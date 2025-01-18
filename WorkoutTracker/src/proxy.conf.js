const PROXY_CONFIG = [
  {
    context: [
      "/api",
      "/resources"
    ],
    target: "https://localhost:40443",
    secure: false,
  }
]

module.exports = PROXY_CONFIG;
