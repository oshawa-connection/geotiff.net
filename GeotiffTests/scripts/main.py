'''
Generates all test files by calling all other python files in this directory.

'''

import sys
import os
import glob
from pathlib import Path

TIF_FILE_TYPES = ['*.tif','*.ovr','*.msk']

scriptPath = Path(__file__).parent
scriptDirectory = Path(__file__).parent
dataDirectory = scriptDirectory.parent.joinpath('data')

for filetype in TIF_FILE_TYPES:
    files = scriptPath.glob(filetype)
    for ff in files:
        origin = scriptPath / ff.name
        print(f"Removing {origin}")
        os.remove(origin)


allPythonFiles = scriptDirectory.glob('*.py')

os.environ["TIF_OUTPUT_DIR"] = str(dataDirectory)

if __name__ == '__main__':
    for pythonFile in allPythonFiles:
        if pythonFile != Path(__file__).name:
            print(f"About to run: {pythonFile}")
            __import__(pythonFile.stem)
    
    
    for filetype in TIF_FILE_TYPES:
        files = scriptPath.glob(filetype)
        for ff in files:
            
            
            origin = scriptPath / ff.name
            destination = dataDirectory / ff.name

            print(f"Moving {origin} -> {destination}")

            # This will replace if exists, or move if not
            os.replace(origin, destination)


