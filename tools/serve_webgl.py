#!/usr/bin/env python3
import http.server
import socketserver
import os

PORT = 8765
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "web2"))


class Handler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=ROOT, **kwargs)

    def end_headers(self):
        # Unity WebGL 产物为 Brotli(.br) 压缩，必须声明 Content-Encoding 让浏览器先解压
        if self.path.endswith(".br"):
            self.send_header("Content-Encoding", "br")
        super().end_headers()

    def guess_type(self, path):
        # 解压后的真实媒体类型（Content-Encoding 解压后浏览器按此处理）
        if path.endswith(".js.br"):
            return "application/javascript"
        if path.endswith(".wasm.br"):
            return "application/wasm"
        if path.endswith(".data.br"):
            return "application/octet-stream"
        if path.endswith(".js"):
            return "application/javascript"
        if path.endswith(".wasm"):
            return "application/wasm"
        return super().guess_type(path)


if __name__ == "__main__":
    socketserver.TCPServer.allow_reuse_address = True
    with socketserver.TCPServer(("0.0.0.0", PORT), Handler) as httpd:
        print(f"Serving {ROOT} on http://localhost:{PORT}")
        httpd.serve_forever()
