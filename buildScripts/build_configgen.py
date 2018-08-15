import os, sys, traceback, shutil, subprocess

sep_char = "\\"
cur_path = os.path.abspath(os.getcwd())
script_path = os.path.dirname(os.path.realpath(__file__))
src_path = os.path.join(script_path, r'../configMgr/Assets/Scripts/'.replace('/', sep_char))
temp_folder = os.path.join(script_path, r'temp')
codegen_exe_out = os.path.join(script_path, r'temp/codegen.exe')
codegen_gencode_out = os.path.join(script_path, r'../configMgr/Assets/GeneratedCode')
codegen_temp_out = os.path.join(temp_folder, r'codegen')
configgen_exe_out = os.path.join(script_path, r'temp/config_binary_maker.exe')
binary_maker_path = os.path.join(script_path, r'../resource/tools/config_binary_maker')

libs = [\
	"UnityEngine.dll",\
	"ICSharpCode.SharpZipLib.dll",\
	"LitJson.dll",\
	"DocumentFormat.OpenXml.dll",\
]

rsp_template = """
-debug
-target:exe
-platform:x64
-warn:0
-unsafe
-main:Alche.Runtime.%s
-define:CODE_GEN
-define:UNITY_5_5_OR_NEWER
%s
-out:%s
%s
%s
"""

excludes = [\
	"/Editor/",\
	"/Standard Assets",\
	"/Art/",\
	"/GeneratedCode"
]

def try_remove(f):
	max_try_count = 20
	while max_try_count != 0:
		max_try_count = max_try_count - 1
		try:
			if os.path.isfile(f):
				os.remove(f)
			else:
				break
		except:
			print 'Try remove %s failed, try again...' % f
			time.sleep(0.25)

def print_with_decode(text, decodefrom, decodeto):
	if decodefrom == decodeto:
		print text
	else:
		print text.decode(decodefrom).encode(decodeto)

def assert_exec(cmd, print_output = True, exit_when_error = True, on_error = None, stdout = subprocess.PIPE, decodefrom = 'gbk'):
	decodeto = 'gbk'

	p = subprocess.Popen(cmd, shell=True, stdout=stdout, stderr=subprocess.STDOUT)
	(stdoutput,erroutput) = p.communicate()
	if(p.returncode != 0 and exit_when_error==True):
		print cmd + " failed!!!"
		print "******************************************************"
		try:
			print_with_decode(stdoutput, decodefrom, decodeto)
		except:
			print_with_decode(stdoutput, decodefrom, decodeto)
		print "******************************************************"
		if on_error != None:
			on_error()
		sys.exit(-1)
	else:
		if(print_output == True):
			try:
				print_with_decode(cmd, "utf-8", decodeto)
			except:
				pass
			print_with_decode(stdoutput, decodefrom, decodeto)
	return stdoutput

def exec_build(args):
	rsp = os.path.join(script_path, 'client.rsp')
	f = open(rsp, 'w')
	f.write(args)
	f.close()
	assert_exec('%s @%s' % ("csc", rsp))
	try_remove(rsp)

def list_dlls(path, dlls):
	lib = ""
	for item in dlls:
		lib += '-r:"%s/%s"\n' % (path, item)
	return lib

def remove_files(folder, ext):
	for (dirpath, dirnames, filenames) in os.walk(folder):
		for file in filenames:
			if file.lower().endswith(ext):
				filepath = os.path.join(dirpath, file)
				os.remove(filepath)

def clean_codeout(temp):
	print ('clean codeout')
	if not os.path.isdir(temp):
		os.mkdir(temp)
	remove_files(temp, '.cs')

def copy_extension(src, dst, extension):
	for file in os.listdir(src):
		if file.endswith(extension):
			shutil.copy(r'%s/%s' % (src, file), dst)

def build_codegen(dirs, disableExcel = False):
	print ('build codegen')
	os.chdir(src_path)
	lib = list_dlls(temp_folder, libs)
	excelBuildParam = "-r:WPF/WindowsBase.dll"
	if disableExcel:
		excelBuildParam = "-define:DISABLE_EXCEL_BUILDER"
	exec_build(rsp_template % ("GameCodeGenApp", excelBuildParam, codegen_exe_out, dirs, lib))
	os.chdir(cur_path)

def run_code_gen(targetNamespaces = ""):
	print ('generate code')
	assert_exec(r'%s %s %s %s' % ("", codegen_exe_out, codegen_temp_out, '"%s"' % targetNamespaces))

def build_binary_maker(dirs):
	print ('build config_binary_maker')
	os.chdir(src_path)
	lib = list_dlls(temp_folder, libs)
	exec_build(rsp_template % ("GameConfigGenApp", "-define:DISABLE_EXCEL_BUILDER", configgen_exe_out, dirs, lib))
	os.chdir(cur_path)

def copy_maker_to_tool():
	print ('copy make to %s' % binary_maker_path)
	shutil.copy(r'temp/config_binary_maker.exe', binary_maker_path);
	shutil.copy(r'temp/config_binary_maker.pdb', binary_maker_path);
	
def copy_codes():
	print ('copy generated code')
	clean_codeout(codegen_gencode_out)
	copy_extension(codegen_temp_out, codegen_gencode_out, ".cs")

def is_exclude(folder, excludes):
	folder = folder + sep_char
	for item in excludes:
		i = item.replace('/', sep_char)
		if folder.find(i) != -1:
			return True
	return False

def list_src_files(folder, excludes = None, prefix = "", tagbegin = "", tagend = ""):
	files = ""
	prefix = prefix.replace('/', sep_char)
	for (dirpath, dirnames, filenames) in os.walk(folder):
		if excludes == None or not is_exclude(dirpath, excludes):
			for file in filenames:
				if file.endswith(".cs"):
					file = dirpath + sep_char + file
					files += "%s\"%s%s\"%s\n" % (tagbegin, prefix, file[len(folder):], tagend)
	return files

if __name__ == '__main__':
	try:
		if not os.path.isdir(temp_folder):
			os.mkdir(temp_folder)
			
		clean_codeout(codegen_temp_out)
		
		result = list_src_files(src_path, excludes)
		
		build_codegen(result)
		run_code_gen()
		
		result = list_src_files(src_path, excludes)
		result += list_src_files(codegen_temp_out, None, "../../../buildScripts/temp/codegen")
		print(result)
		
		build_binary_maker(result)
		# copy_maker_to_tool()
		copy_codes()
		os.system('pause')
	except:
		traceback.print_exc()
		os.system('pause')
		sys.exit(-1)