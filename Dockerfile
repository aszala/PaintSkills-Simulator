FROM ubuntu:16.04

ENV PATH /usr/local/bin:$PATH

RUN apt-get update && apt-get -y upgrade
RUN apt-get -y install zip
RUN apt-get install -y xvfb
RUN apt-get -y install libxtst6 libxv1 libglu1-mesa


RUN mkdir /home/sim/
RUN mkdir /home/sim/output/

COPY linux_build.zip /home/sim/sim.zip

WORKDIR /home/sim
RUN unzip sim.zip

WORKDIR /home/sim/Linux
COPY start.sh /home/sim/Linux/start.sh
RUN chmod +x /home/sim/Linux/build.x86_64
RUN chmod +x /home/sim/Linux/start.sh

RUN mkdir /home/sim/scenes/

ENTRYPOINT ["sh", "/home/sim/Linux/start.sh"]
