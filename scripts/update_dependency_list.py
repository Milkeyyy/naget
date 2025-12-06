import copy
import json
import traceback


try:
	with open("../naget/library.json", "r") as f:
		lib_list = json.loads(f.read())
		custom_lib_list = copy.deepcopy(lib_list)
	with open("./custom.json", "r") as f:
		custom_list = json.loads(f.read())
	for cs_name in custom_list.keys():
		for count, lib in enumerate(lib_list, 0):
			if lib["PackageId"] == cs_name:
				custom_lib_list[count] = custom_list[cs_name]
				break
		else:
			custom_lib_list.append(custom_list[cs_name])
		
	with open("../naget/library.json", "w") as f:
		f.write(json.dumps(custom_lib_list, indent=4))
except Exception:
	traceback.print_exc()
