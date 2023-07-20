# Semantic Kernel Sidecar

This Repo is mean to be an example of setting up Semantic Kernel (SK) as a Sidecar Process (Container) that will attempt to abstract the key functionality of SK via HTTP and gRPC, such that a developer does not require loading or including a native library/SDK to work with SK.

## Design Goals

Following similar design principles adopted from other sidecar/proxy type projects (e.g. Dapr.io), we want to follow a convention for using the sidecar.

1. Configuration (AOAI/OAI connection strings and settings) are loaded via a secret/config map file in the container's file path (/config/settings.json)
1. All "Semantic Skills" are loaded in via single shared file store, where each skill is it's own separately named folder that contains two files (eg. skills/WriterSkill/[config.json, skprommpt.txt]).  The goal being that skills can be added, updated and loaded via this folder structure convention even during run time.  This means there are no inline Semantic Templates and templates will only be loaded via the shared/loaded file share (e.g. Azure Files or Blob Storage)