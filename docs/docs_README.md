# 📚 MpyjVPN Documentation

Welcome to the MpyjVPN documentation hub.

## 📖 Table of Contents

| Document | Description |
|----------|-------------|
| [Architecture](ARCHITECTURE.md) | Clean Architecture overview |
| [Contributing](CONTRIBUTING.md) | How to contribute |
| [Protocols](PROTOCOLS.md) | All 14 protocols explained |
| [Build Guide](../README.md#-build-from-source) | How to build |

## 🎯 Quick Links

- [Main README](../README.md)
- [Latest Release](https://github.com/Mpyj/MpyjVPN/releases/latest)
- [Issue Tracker](https://github.com/Mpyj/MpyjVPN/issues)
- [Discussions](https://github.com/Mpyj/MpyjVPN/discussions)

## 🏗️ Architecture at a Glance

MpyjVPN is built with **Clean Architecture** (6 layers):

1. **Domain** — Models, Enums, Interfaces
2. **Core** — VPN Engine, 14 Protocols
3. **Infrastructure** — Parsers, Storage
4. **Application** — ViewModels, Services
5. **Avalonia** — UI (Views, Controls)
6. **CLI** — Command-line

See [ARCHITECTURE.md](ARCHITECTURE.md) for details.

## 🔌 Protocols

| # | Protocol | Description |
|---|----------|-------------|
| 1 | WARP | Cloudflare WARP |
| 2 | DoH | DNS over HTTPS |
| 3 | Fragment | TLS fragmentation |
| 4 | Worker | Cloudflare Worker |
| 5 | Google Proxy | Google Translate |
| 6 | ECH | Encrypted Client Hello |
| 7 | QUIC | HTTP/3 |
| 8 | ICMP | ICMP tunneling |
| 9 | WebSocket | WS tunneling |
| 10 | Fronting | Domain Fronting |
| 11 | Multi-hop | Chain hops |
| 12 | DNS Tunnel | DNS tunneling |
| 13 | IP Spoof | IP spoofing |
| 14 | SNI Spoof | SNI spoofing |

## 🛠️ Tech Stack

- **.NET 10**
- **Avalonia 11.2.1**
- **C# 13**
- **Clean Architecture**

## 📞 Need Help?

- Open an [issue](https://github.com/Mpyj/MpyjVPN/issues)
- Start a [discussion](https://github.com/Mpyj/MpyjVPN/discussions)
