import sys
import os
import glob
from pathlib import Path

print(__file__)
scriptPath = Path(__file__).parent
scriptDirectory = Path(__file__).parent.parent
dataDirectory = scriptDirectory.joinpath('data')

print(dataDirectory)

allPythonFiles = scriptDirectory.glob('*.py')

if __name__ == '__main__':
    for pythonFile in allPythonFiles:
        if pythonFile != Path(__file__).name:
            __import__(pythonFile.stem)
    
    filetypes = ['*.tif','*.ovr','*.msk']
    for filetype in filetypes:
        files = scriptPath.glob(filetype)
        for ff in files:
            
            
            origin = scriptPath / ff.name
            destination = dataDirectory / ff.name

            print(f"Moving {origin} → {destination}")

            # This will replace if exists, or move if not
            os.replace(origin, destination)


