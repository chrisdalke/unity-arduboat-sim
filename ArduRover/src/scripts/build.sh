#!/bin/bash

# Build instructions from http://ardupilot.org/dev/docs/setting-up-sitl-on-linux.html

set -e
DEBIAN_FRONTEND=noninteractive

git config --global url."https://github.com/".insteadOf git://github.com/
git clone https://github.com/ArduPilot/ardupilot.git ardupilot

cd ardupilot
git checkout Rover-4.3
Tools/gittools/submodule-sync.sh
USER=nobody /install-prereqs-ubuntu.sh -y

./waf distclean
./waf configure --board sitl
./waf rover