from http.server import SimpleHTTPRequestHandler, HTTPServer
import os

class SingleRangeHandler(SimpleHTTPRequestHandler):
    '''
    If the client requests multiple ranges, only return the first. 
    '''
    def send_head(self):
        path = self.translate_path(self.path)
        f = open(path, 'rb')
        fs = os.fstat(f.fileno())
        size = fs.st_size

        range_header = self.headers.get("Range")
        if range_header:
            # Example: "bytes=0-10,20-30"
            ranges = range_header.replace("bytes=", "").split(",")
            first_range = ranges[0].strip()

            start, end = first_range.split("-")
            start = int(start) if start else 0
            end = int(end) if end else size - 1

            if end >= size:
                end = size - 1

            self.send_response(206)
            self.send_header("Content-Type", "application/octet-stream")
            self.send_header("Content-Range", f"bytes {start}-{end}/{size}")
            self.send_header("Content-Length", str(end - start + 1))
            self.end_headers()

            f.seek(start)
            self.wfile.write(f.read(end - start + 1))
            f.close()
            return None

        # No Range → normal behavior
        return super().send_head()

if __name__ == "__main__":
    HTTPServer(("0.0.0.0", 8001), SingleRangeHandler).serve_forever()