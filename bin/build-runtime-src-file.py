#!/usr/bin/env python

################################################################################

src_algs                = 'core/algs.cs'
src_builder             = 'core/builder.cs'
src_iters               = 'core/iters.cs'
src_miscellanea         = 'core/miscellanea.cs'
src_objs                = 'core/objs.cs'
src_parser              = 'core/parser.cs'
src_seqs                = 'core/seqs.cs'
src_symb_table          = 'core/symb-table.cs'
src_value               = 'core/value.cs'

src_procs               = 'procs.cs'

src_table               = 'automata/binary-table.cs'
src_index               = 'automata/index.cs'
src_one_way_bin_table   = 'automata/one-way-bin-table.cs'
src_ternary_table       = 'automata/ternary-table.cs'
src_unary_table         = 'automata/unary-table.cs'
src_value_store         = 'automata/value-store.cs'

src_conversions         = 'wrapping/conversions.cs'
src_wrapping_utils      = 'wrapping/wrapping-utils.cs'

################################################################################

std_sources = [
  src_algs,
  src_builder,
  src_iters,
  src_miscellanea,
  src_objs,
  src_parser,
  src_seqs,
  src_symb_table,
  src_value,

  src_procs
]

table_sources = [
  src_table,
  src_index,
  src_one_way_bin_table,
  src_ternary_table,
  src_unary_table,
  src_value_store
]

interface_sources = [
  src_conversions,
  src_wrapping_utils
]

################################################################################

num_of_tabs = 0

def escape(ch):
  if ch == ord('\\'):
    return '\\\\'
  elif ch == ord('"'):
    return '\\"'
  elif ch >= ord(' ') or ch <= ord('~'):
    return chr(ch)
  elif ch == ord('\t'):
    global num_of_tabs
    num_of_tabs += 1
    return '\\t'
  else:
    print 'Invalid character: ' + ch
    exit(1);


def convert_file(file_name):
  res = []
  f = open(file_name)
  # i = 0
  for l in f:
    l = l.rstrip()
    # if i > 10:
    #   break
    # i = i + 1
    if l.startswith('using'):
      pass
    else:
      el = ''.join([escape(ord(ch)) for ch in l])
      res.append('"' + el + '"')
  return res


# def to_code(bytes):
#   count = len(bytes)
#   ls = []
#   l = ' '
#   for i, b in enumerate(bytes):
#     l += ' ' + str(b) + (',' if i < count-1 else '')
#     if len(l) > 80:
#       ls.append(l)
#       l = ' '
#   if l:
#     ls.append(l)
#   return ls


def convert_files(directory, file_names):
  ls = []
  for i, f in enumerate(file_names):
    if i > 0:
      ls.extend(['""', '""'])
    ls.extend(convert_file(directory + '/' + f))
  last_line = len(ls) - 1
  fls = []
  for i, l in enumerate(ls):
    if i != last_line:
      l += ','
    fls.append('  ' + l)
  return fls


def data_array_def(array_name, directory, file_names):
  data = convert_files(directory, file_names)
  # code = to_code(data)
  return ['String* ' + array_name + ' = ('] + data + [');']

################################################################################

from sys import argv, exit

if len(argv) != 4:
  print 'Usage: ' + argv[0] + ' <input directory> <output file> <empty output file>'
  exit(0)

_, input_dir, out_fname, empty_out_fname = argv

file_data = [
  data_array_def('core_runtime', input_dir, std_sources),
  data_array_def('table_runtime', input_dir, table_sources),
  data_array_def('interface_runtime', input_dir, interface_sources)
]

out_file = open(out_fname, 'w')
for i, f in enumerate(file_data):
  if i > 0:
    out_file.write('\n\n')
  for l in f:
    out_file.write(l + '\n');

empty_file_data = [
  data_array_def('core_runtime', input_dir, []),
  data_array_def('table_runtime', input_dir, []),
  data_array_def('interface_runtime', input_dir, [])
]

empty_out_file = open(empty_out_fname, 'w')
for i, f in enumerate(empty_file_data):
  if i > 0:
    empty_out_file.write('\n\n')
  for l in f:
    empty_out_file.write(l + '\n')
