import os
from os import listdir
from os.path import isfile, join
from fbrecog import recognize
from prettytable import PrettyTable
import sys

# Authentication factors
lines = sys.stdin.read().splitlines()
access_token = lines[0].split()[0]
cookie = lines[1]
fb_dtsg = lines[2]

def get_sum(array):
	sum = 0.0;

	for i in xrange(0, len(array)):
		value = float(array[i]['certainity']);
		sum += value

	return sum

def populate_array_diagnostics(table, result):
	for i in xrange(0, len(result)):
		table.add_row(["" , "", result[i]['name'][0:12], str(result[i]['certainity'])[0:5]])

def print_diagnostics(image, input_result, output_result):
	table = PrettyTable(["File", "Status", "Name", "Certainity"])
	table.align["Name"] = "l"
	table.border = False
	table.header = False

	table.add_row([image, "", "", ""])

	table.add_row(["", "Input", "", ""])
	populate_array_diagnostics(table, input_result)

	table.add_row(["", "Output", "", ""])
	populate_array_diagnostics(table, output_result)

	print table

def concat_array(array):
	result = "";
	for i in xrange(0, len(array)):
		result += array[i] + " "

	return result

def load_array(cached_path):
	array = []
	with open(cached_path) as file:
		n_faces = int(next(file).split()[0])

		for i in xrange(0, n_faces):
			face = {}
			result = next(file).split()
			face['certainity'] = result[0]
			face['name'] = concat_array(result[1:])
			array.append(face)

	return array

def store_array(array, path):
	with open(path, 'w') as file:
		file.write(str(len(array)) + '\n')

		for i in xrange(0, len(array)):
			line = str(array[i]['certainity']) + ' ' + array[i]['name']
			file.write(line + '\n')

		file.close()

def get_recognition(image):
	# if we have a cached version, load it
	cached_path = "cache/" + image + "-recognition.txt"
	if not os.path.isfile(cached_path):
		print "\tCaching " + image
		input_result = recognize("input/" + image, access_token, cookie, fb_dtsg)
		store_array(input_result, cached_path)

	return load_array(cached_path)

input_sum = 0.0
output_sum = 0.0
n_lost_faces = 0
n_total_faces = 0

for image in listdir("output/"):
	input_result = get_recognition(image)
	input_sum += get_sum(input_result)

	output_result = recognize("output/" + image, access_token, cookie, fb_dtsg)
	output_sum += get_sum(output_result)

	n_lost_faces += len(input_result) - len(output_result)
	n_total_faces += len(input_result)

	print_diagnostics(image, input_result, output_result)

# Printing final metrics
print "\n\n"

final_metrics = PrettyTable(["Feature", "Value"])
final_metrics.border = False
final_metrics.header = False
final_metrics.align = "l"
final_metrics.add_row(["Disguised faces", n_lost_faces])
final_metrics.add_row(["Total faces", n_total_faces])
final_metrics.add_row(["Initial certainity", input_sum])
final_metrics.add_row(["Final certainity", output_sum])

success_metric = 1.0 - (output_sum / input_sum)
final_metrics.add_row(["Success metric", success_metric])

print final_metrics