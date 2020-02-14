
FROM mcr.microsoft.com/mssql/server

USER root

RUN wget -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get -y install apt-transport-https && \
    apt-get -y install dotnet-sdk-2.2

ENV ACCEPT_EULA=Y
ARG MSSQL_SA_PASSWORD=SA_PASSWORD1.
ARG DB_PREFIX=TEST_

WORKDIR /usr/bin/rdmp-cli

COPY ./Tools/rdmp/bin/Debug/netcoreapp2.2/linux-x64/publish .
COPY ./scripts/install-rdmp-docker.sh .
RUN ./install-rdmp-docker.sh
RUN rm ./install-rdmp-docker.sh
