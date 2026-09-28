const http = require("http");

const port = Number(process.env.PORT || 8000);

const server = http.createServer((req, res) => {
  if (req.url === "/health") {
    res.writeHead(200, { "Content-Type": "application/json" });
    res.end(JSON.stringify({ status: "ok", service: "api" }));
    return;
  }

  res.writeHead(200, { "Content-Type": "application/json" });
  res.end(JSON.stringify({ message: "API is running" }));
});

server.listen(port, "0.0.0.0", () => {
  console.log(`API listening on ${port}`);
});
