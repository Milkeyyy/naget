#!/bin/bash
# naget のコード署名に使う自己署名証明書と codesign ラッパーを用意する。
# 証明書が変わると TCC (アクセシビリティ) の許可が失効するため、CI と共用すること。
# ラッパーは vpk が固定で付ける --timestamp を外す (自己署名では Apple の TSA を待って止まることがある)。
# CI 用の p12 は次のコマンドで書き出し、base64 にして MACOS_CERT_P12 / MACOS_CERT_PASSWORD に登録する:
#   security export -k ~/Library/Keychains/login.keychain-db -t identities -f pkcs12 -P <パスワード> -o cert.p12
set -euo pipefail

IDENTITY="naget Self Signed"
KEYCHAIN="${HOME}/Library/Keychains/login.keychain-db"
BIN_DIR="${TMPDIR:-/tmp}/naget-signing/bin"

mkdir -p "${BIN_DIR}"
cat > "${BIN_DIR}/codesign" <<'EOF'
#!/bin/bash
args=()
for arg in "$@"; do
    if [ "${arg}" != "--timestamp" ]; then
        args+=("${arg}")
    fi
done
exec /usr/bin/codesign "${args[@]}"
EOF
chmod +x "${BIN_DIR}/codesign"

if security find-identity -v -p codesigning | grep -q "${IDENTITY}"; then
    echo "${BIN_DIR}"
    exit 0
fi

# 証明書はあるが信頼されていない場合は信頼だけ設定し直す
if security find-certificate -c "${IDENTITY}" >/dev/null 2>&1; then
    security find-certificate -c "${IDENTITY}" -p > "${BIN_DIR}/existing.pem"
    security add-trusted-cert -r trustRoot -p codeSign -k "${KEYCHAIN}" "${BIN_DIR}/existing.pem" >/dev/null 2>&1 || true
    rm -f "${BIN_DIR}/existing.pem"
    if security find-identity -v -p codesigning | grep -q "${IDENTITY}"; then
        echo "${BIN_DIR}"
        exit 0
    fi
fi

TMP_DIR="$(mktemp -d)"
trap 'rm -rf "${TMP_DIR}"' EXIT

cat > "${TMP_DIR}/openssl.cnf" <<EOF
[req]
distinguished_name = dn
x509_extensions = v3
prompt = no
[dn]
CN = ${IDENTITY}
[v3]
keyUsage = critical, digitalSignature
extendedKeyUsage = critical, codeSigning
basicConstraints = critical, CA:false
EOF

openssl req -x509 -newkey rsa:2048 -nodes -days 3650 \
    -keyout "${TMP_DIR}/key.pem" -out "${TMP_DIR}/cert.pem" -config "${TMP_DIR}/openssl.cnf" >/dev/null 2>&1
openssl pkcs12 -export -out "${TMP_DIR}/cert.p12" -inkey "${TMP_DIR}/key.pem" \
    -in "${TMP_DIR}/cert.pem" -passout pass:naget >/dev/null 2>&1

# OpenSSL 3 の既定 (AES-256) は security import できないため -legacy で作り直す
if ! security import "${TMP_DIR}/cert.p12" -k "${KEYCHAIN}" -P naget -T /usr/bin/codesign >/dev/null 2>&1; then
    openssl pkcs12 -export -legacy -out "${TMP_DIR}/cert.p12" -inkey "${TMP_DIR}/key.pem" \
        -in "${TMP_DIR}/cert.pem" -passout pass:naget >/dev/null 2>&1
    security import "${TMP_DIR}/cert.p12" -k "${KEYCHAIN}" -P naget -T /usr/bin/codesign >/dev/null 2>&1
fi

# codesign が証明書を信頼済みとして扱えるようにする (失敗しても署名自体は可能)
security add-trusted-cert -r trustRoot -p codeSign -k "${KEYCHAIN}" "${TMP_DIR}/cert.pem" >/dev/null 2>&1 || true

if ! security find-identity -v -p codesigning | grep -q "${IDENTITY}"; then
    echo "${IDENTITY} を作成できませんでした" >&2
    exit 1
fi

echo "CI 用の証明書: security export -k \"${KEYCHAIN}\" -t identities -f pkcs12 -P <パスワード> -o cert.p12" >&2
echo "${BIN_DIR}"
