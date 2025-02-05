'''

'''

import os
import pathlib

SHELL = 'pandoc -s [MD] -o [RTF]'
SPLITER = '/'
if os.name == 'nt':
    SPLITER = '\\'

def scandir_recurrence(dir_path):
    # scan directory
    for entry in os.scandir(dir_path):
        # full path
        path = os.path.join(dir_path, entry.name)
        
        # Modify path from MD/... to RTF/...
        path_split = path.split(SPLITER)
        rtf_path = os.path.join('RTF', SPLITER.join(path_split[1:]))

        if entry.is_dir(): # directory
            if not os.path.exists(rtf_path):
                os.mkdir(rtf_path)

            # recurrence all sub directory
            scandir_recurrence(path)

        elif entry.is_file(): # file
            # convert md file from ./MD/... and output rtf file to ./RTF/...
            shell = SHELL.replace('[MD]', path).replace('[RTF]', rtf_path.replace('.md', '.rtf')) 

            # execute shell command
            os.system(shell)

def main():
    folder = 'RTF'
    if not os.path.exists(folder):
        os.mkdir(folder)

    scandir_recurrence('MD')

if __name__ == '__main__':
    main()