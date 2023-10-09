FROM ubuntu:20.04

ARG COPTER_TAG=Rover-4.3

# install git 
RUN apt-get update && apt-get install -y git; git config --global url."https://github.com/".insteadOf git://github.com/

# Add Tini
ENV TINI_VERSION v0.19.0
ADD https://github.com/krallin/tini/releases/download/${TINI_VERSION}/tini /tini
RUN chmod +x /tini
ENTRYPOINT ["/tini", "--"]

# Now grab ArduPilot from GitHub
RUN git clone https://github.com/ArduPilot/ardupilot.git ardupilot
WORKDIR ardupilot

# Checkout the latest Copter...
RUN git checkout ${COPTER_TAG}

# Now start build instructions from http://ardupilot.org/dev/docs/setting-up-sitl-on-linux.html
RUN git submodule update --init --recursive

# Trick to get apt-get to not prompt for timezone in tzdata
ENV TZ=UTC
ENV DEBIAN_FRONTEND=noninteractive

# Need sudo and lsb-release for the installation prerequisites
RUN apt-get install -y sudo lsb-release tzdata

# Need USER set so usermod does not fail...
# Install all prerequisites now
COPY ./src/scripts/install-prereqs-ubuntu.sh Tools/environment_install/install-prereqs-ubuntu-docker.sh
RUN USER=nobody Tools/environment_install/install-prereqs-ubuntu-docker.sh -y

# Continue build instructions from https://github.com/ArduPilot/ardupilot/blob/master/BUILD.md
RUN ./waf distclean
RUN ./waf configure --board sitl
RUN ./waf copter
RUN ./waf rover 
RUN ./waf plane
RUN ./waf sub

# TCP 5760 is what the sim exposes by default
EXPOSE 5760/tcp

# Variables for simulator
ENV INSTANCE 0
ENV LAT 42.3898
ENV LON -71.1476
ENV ALT 14
ENV DIR 270
ENV MODEL +
ENV SPEEDUP 1
ENV VEHICLE ArduCopter

# Finally the command

WORKDIR /
COPY ./src/scripts/launch.sh .
CMD ["/launch.sh"]
