# MpyjVPN

<p align="center">
  <strong>Fast, Secure & Modern VPN Client</strong><br>
  A modern cross-platform VPN client built with C# and Avalonia.
</p>

<p align="center">
  <a href="README.Fa.md">🇮🇷 نسخه فارسی</a>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet" alt=".NET 10">
  <img src="https://img.shields.io/badge/Avalonia-11.2.1-8B44AC?style=flat-square" alt="Avalonia 11.2.1">
  <img src="https://img.shields.io/badge/Language-C%23-239120?style=flat-square&logo=csharp" alt="C#">
  <img src="https://img.shields.io/badge/License-MIT-green?style=flat-square" alt="MIT License">
</p>

MpyjVPN is a modern, cross-platform VPN client designed with a clean architecture, a polished user interface, and a flexible protocol and configuration system.

It brings multiple connection methods, configuration importing, diagnostics, DNS management, live connection information, and a modern desktop experience together in one application.

---

## 📸 Screenshots

### Home

![MpyjVPN Home](assets/screenshots/home.jpg)

### Connected

![MpyjVPN Connected](assets/screenshots/connected.jpg)

### Protocols

![MpyjVPN Protocols](assets/screenshots/protocols.jpg)

### Settings

![MpyjVPN Settings](assets/screenshots/settings.jpg)

### Diagnostics

![MpyjVPN Diagnostics](assets/screenshots/diagnostics.jpg)

---

## ✨ Features

### 🌐 Built-in Protocols

MpyjVPN includes **14 built-in protocols and techniques**:

- WARP
- DoH
- Fragment
- Worker
- Google Proxy
- ECH
- QUIC
- ICMP
- WebSocket
- Domain Fronting
- Multi-hop
- DNS Tunnel
- IP Spoof
- SNI Spoof

### 🔀 Connection Modes

Choose between **7 connection modes**:

- Auto
- Onion
- Protocols
- Ultra
- WARP-in-WARP
- WARP+Worker
- Full Stack

The connection engine can combine different layers and techniques into flexible connection stacks.

### 🧩 Advanced Connection Stack

- **18 stack layers**
- WARP-in-WARP
- Multi-Hop connections
- Automatic reconnect
- Live connection information
- Ping and packet-loss monitoring
- IP scanner
- DNS management

### 📥 Configuration Import

Import existing configurations from multiple sources:

- VMess
- VLess
- Trojan
- Shadowsocks
- JSON
- Subscription URLs
- Base64-encoded subscriptions
- Clipboard auto-import

### 🎨 Modern User Interface

- Dark and Light themes
- Persian RTL support
- English support
- System tray integration
- Toast notifications
- Animated PowerOrb
- Particle Canvas effects
- Clean and responsive interface

### 🛠️ Developer & Diagnostic Tools

- Shared `LogService`
- Diagnostics
- Settings management
- `MpyjCLI`
- Structured project architecture

---

## 🖥️ Supported Platforms

MpyjVPN currently provides self-contained builds for:

| Platform | Architecture |
|---|---|
| Windows | x64 |
| Linux | x64 |
| Linux | ARM64 |
| macOS | Intel |
| macOS | ARM64 |

> Mobile versions are planned for a future release.

---

## 📦 Download

The latest release is available from the project's GitHub Releases page.

### v1.0.0-beta

Self-contained builds are provided for the supported desktop platforms, so users do not need to install .NET separately.

---

## 🏗️ Architecture

MpyjVPN follows **Clean Architecture** and is organized into six main projects:

```text
MpyjVPN/
├── src/
│   ├── MpyjVPN.Domain
│   ├── MpyjVPN.Core
│   ├── MpyjVPN.Infrastructure
│   ├── MpyjVPN.Application
│   ├── MpyjVPN.Avalonia
│   └── MpyjVPN.CLI
├── assets/
│   └── screenshots/
├── docs/
├── README.md
└── README.Fa.md
```

### Project Layers

| Project | Responsibility |
|---|---|
| **Domain** | Models, enums, and interfaces |
| **Core** | VPN engine, protocols, and core services |
| **Infrastructure** | Configuration parsers and storage |
| **Application** | ViewModels, application services, and business logic |
| **Avalonia** | Desktop user interface |
| **CLI** | Command-line interface and developer tooling |

---

## 🧰 Technologies

- **C# 13**
- **.NET 10**
- **Avalonia 11.2.1**
- Clean Architecture
- NuGet
- `dotnet` CLI
- GitHub Actions
- Inno Setup
- `dotnet format`
- CodeQL
- Tmds.DBus.Protocol

---

## 🚀 Build from Source

### Requirements

Make sure you have the required .NET SDK installed.

### Clone the repository

```bash
git clone https://github.com/Mpyj/MpyjVPN.git
cd MpyjVPN
```

### Restore dependencies

```bash
dotnet restore
```

### Build

```bash
dotnet build -c Release
```

### Run the desktop application

```bash
dotnet run --project src/MpyjVPN.Avalonia
```

### Publish

#### Windows x64

```bash
dotnet publish src/MpyjVPN.Avalonia -c Release -r win-x64 --self-contained true
```

#### Linux x64

```bash
dotnet publish src/MpyjVPN.Avalonia -c Release -r linux-x64 --self-contained true
```

#### macOS ARM64

```bash
dotnet publish src/MpyjVPN.Avalonia -c Release -r osx-arm64 --self-contained true
```

---

## 🗺️ Roadmap

### v1.0.0-beta

- Core VPN functionality
- Built-in protocols
- Configuration importing
- Diagnostics
- Desktop UI
- Cross-platform desktop builds

### v1.1.0

- Linux `.deb` package
- macOS `.dmg`
- Code signing
- Auto-update
- SOCKS5 / HTTP Proxy support

### v2.0.0

- Android
- iOS
- Web Dashboard
- Plugin system

> The roadmap may evolve as development continues.

---

## 🤝 Contributing

Contributions, ideas, bug reports, and suggestions are welcome.

If you find a problem or have an idea that could improve MpyjVPN, feel free to open an issue or submit a pull request.

---

## 📄 License

MpyjVPN is released under the **MIT License**.

See the `LICENSE` file for the full license text.

---

## 🌐 Connect with Mpyj

<p>
  <a href="https://x.com/Mpyj_X">𝕏 X</a> ·
  <a href="https://t.me/MpyjTelegram">✈️ Telegram</a>
</p>

---

<p align="center">
  <strong>MpyjVPN</strong><br>
  Built by Mpyj
</p>
