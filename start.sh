#!/bin/sh
xvfb-run --auto-servernum --server-args='-screen 0 720x720x24 -ac +extension GLX +render -noreset' /home/sim/Linux/build.x86_64 "$@" -logFile log.log
