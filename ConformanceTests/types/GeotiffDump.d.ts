export type PixelInfo = {[x: number, y: number, bandInfo: Array<number>]};


export type GeotiffImage = {
    tags:  { [id: string] : any; },
    pixels: PixelInfo[]
}



export type GeotiffDump = {
    fileName: string,
    images:GeotiffImage[]
}