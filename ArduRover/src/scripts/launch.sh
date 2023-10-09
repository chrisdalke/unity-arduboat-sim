#!/bin/bash

if [ "$RUN_SIM" = false ] ; then
    echo "Idling"
    tail -f /dev/null
fi

echo "Starting ArduPilot"
/ardupilot/Tools/autotest/sim_vehicle.py \
   --vehicle APMrover2 \
   -I0 \
   -f json:127.0.0.1 \
   --custom-location=42.3898,-71.1476,0.0,0.0 \
   --console