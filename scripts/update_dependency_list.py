import json
import traceback


try:
	print("ライブラリー一覧更新開始")

	custom_lib_list = []

	with open("../naget/library.json", "r", encoding="utf-8") as f:
		lib_list = json.loads(f.read())

	with open("./custom.json", "r", encoding="utf-8") as f:
		custom_list = json.loads(f.read())

	for lib in lib_list:
		if lib["PackageId"] in custom_list: # カスタム
			if custom_list[lib["PackageId"]] is None: # None (null) の場合は除外
				print(f"- 除外: {lib["PackageId"]}")
			else:
				print(f"- カスタム: {lib["PackageId"]}")
				custom_lib_list.append(custom_list[lib["PackageId"]])
			# 既に追加したので削除
			custom_list.pop(lib["PackageId"])
		else: # そのまま
			custom_lib_list.append(lib)

	# 残ったライブラリー (元の一覧には存在しないもの) を追加
	for c_lib in custom_list.values():
		print(f"- カスタム: {c_lib["PackageId"]}")
		custom_lib_list.append(c_lib)

	custom_lib_list.sort(key=lambda x: x["PackageId"])

	# for cs_name in custom_list.keys():
	# 	for count, lib in enumerate(lib_list, 0):
	# 		if custom_list[cs_name] is None: # 除外対象
	# 			continue
	# 		if lib["PackageId"] == cs_name:
	# 			custom_lib_list.append(custom_list[cs_name])
	# 			break
	# 	else:
	# 		custom_lib_list.append(custom_list[cs_name])

	with open("../naget/library.json", "w") as f:
		f.write(json.dumps(custom_lib_list, indent=4))

	print("ライブラリー一覧更新終了")
except Exception:
	traceback.print_exc()
