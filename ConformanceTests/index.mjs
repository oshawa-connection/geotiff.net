//@ts-check
import fs, {promises} from 'node:fs'
import GeoTIFF, {fromArrayBuffer} from 'geotiff';
import path from 'path';


/**
 * @type {import('./types/GeotiffDump').GeotiffDump[]}
 */
const writeResult = [];


/**
 * Transform typed arrays into raw arrays for better serialization
 * @param {*} fileDirectory 
 */
function serializeTags(fileDirectory) {
  const result = {};
  Object.keys(fileDirectory).forEach(key => {
    let value = fileDirectory[key];
    if (ArrayBuffer.isView(value)) {
      value = Array.from(value);
    }
    result[key] = value;
  })
  return result;
}


async function produceStats(filePath) {
    const data = await promises.readFile(filePath);
    
    // const data = fs.readFileSync('./tiffData/lat_lon_grid.tif');
    const result = await fromArrayBuffer(data.buffer);
    /**
     * @type {import('./types/GeotiffDump').GeotiffDump}
     */
    const currentWriteResult = {fileName: filePath, images: []};
    for(let i = 0; i < await result.getImageCount(); i++) {
        const image = await result.getImage(i);

        let pixelX = 10;
        let pixelY = 10;

        const width = image.getWidth();
        const height = image.getHeight();

        if (width < pixelX) {
            pixelX = width - 1;
            
        }
        if (height < pixelY) {
            pixelY = height - 1;
        }
        const readResult = await image.readRasters({window:[pixelX, pixelY, pixelX+1, pixelY+1]});
        
        const pixelInfo = {x: pixelX, y: pixelY, bandInfo: readResult.map(d => d[0])};
        
        const finalTags = serializeTags(image.fileDirectory);
        currentWriteResult.images.push({tags: finalTags, pixels: [pixelInfo]});
    }
    
    
    writeResult.push(currentWriteResult);
}

// await produceStats('./tiffData/lat_lon_grid.tif');
// console.log(JSON.stringify(writeResult));

async function getFiles(dir, ext, fileList = []) {
  const files = await fs.promises.readdir(dir, { withFileTypes: true });

  for (const file of files) {
    const fullPath = path.join(dir, file.name);
    if (file.isDirectory()) {
      await getFiles(fullPath, ext, fileList); // recurse
    } else if (path.extname(file.name).toLowerCase() === ext.toLowerCase()) {
      fileList.push(fullPath);
    }
  }

  return fileList;
}


const result = await getFiles('./tiffData', '.tif');
const writepromises = result.map(d => produceStats('./' + d));

await Promise.all(writepromises);

await promises.writeFile('writeResult.json', JSON.stringify(writeResult, null, 2));