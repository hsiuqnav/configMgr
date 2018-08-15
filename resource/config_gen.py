#! python2
import os
import sys
sys.path.append(r'../client/build/')

sep_char = "\\"
script_path = os.path.dirname(os.path.realpath(__file__))
base_path = os.path.abspath(r"%s/../".replace('/', sep_char) % script_path)
resource_path = os.path.abspath(r"%s/resource".replace('/', sep_char) % base_path)
cur_path = os.path.abspath(os.getcwd())

readXmlThread = True


def codegen():
	global readXmlThread
	execute = ""

	os.chdir("%s/tools/config_binary_maker/" % resource_path)
	if os.system(("%s config_binary_maker.exe %s" % (execute, readXmlThread)).replace("/", os.sep)) != 0:
		raise Exception("config_binary_maker.exe error!")
	os.chdir(cur_path)

if __name__ == "__main__":
	try:
		quiet = False
		if len(sys.argv) > 1:
			quiet = sys.argv[1] == "-q"
		codegen()
		print ('config gen complete')
		os.system('pause')

	except Exception as e:
		print ('config gen failed: ')
		print (e)
		os.system('pause')
		sys.exit(-1)
