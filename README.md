# GateGuard NFive Plugin
[![License](https://img.shields.io/github/license/NFive/plugin-gateguard.svg)](LICENSE)
[![Build Status](https://img.shields.io/appveyor/ci/NFive/plugin-gateguard.svg)](https://ci.appveyor.com/project/NFive/plugin-gateguard)
[![Release Version](https://img.shields.io/github/release/NFive/plugin-gateguard/all.svg)](https://github.com/NFive/plugin-gateguard/releases)

This plugin allows you to control who can connect to your [NFive](https://github.com/NFive) [FiveM](https://fivem.net/) GTAV server with either a whitelist or blacklist.

Rules can be permanently added in the YAML configuration file as well as dynamically managed in the database.

## Installation
Install the plugin into your server from the [NFive Hub](https://hub.nfive.io/NFive/plugin-gateguard): `nfpm install NFive/plugin-gateguard`

## Configuration
```yml
# Mode can be "whitelist" or "blacklist"
mode: whitelist

# Message to show users who are blocked
message: You are not whitelisted

# Require players to to have a Steam ID
steam:
  required: true
  message: You must be running Steam to play on this server

# Permanent general access rules
rules:
  # IP addresses
  ips:
  - 127.0.0.1

  # FiveM license keys
  licenses:
  - abcdef1234567890abcdef1234567890abcdef12

  # Steam IDs in ID64 format
  # Use https://steamid.io to convert formats
  steam:
  - 12345678901234567

database:
  reload_interval: 00:30:00
```
