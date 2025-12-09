from macos_pkg_builder import Packages


args = sys.argv

runtime = args[0]
version = args[1]

pkg_obj = Packages(
	pkg_output="naget.pkg",
	pkg_bundle_id="com.milkeyyy.naget",
	pkg_version=version,
	pkg_file_structure={
		f"_Pack/{runtime}/naget.app": "/Applications/naget.app",
	},
)

assert pkg_obj.build() is True