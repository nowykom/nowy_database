#!/bin/bash

set -euxo pipefail

docker image build -f Nowy.Database.Web/Dockerfile ..   -t tobiasschulzdev/nowy_database:latest   -t tobiasschulzdev/lr_database:latest
docker image build -f nowy_messagehub/Dockerfile ..     -t tobiasschulzdev/nowy_messagehub:latest -t tobiasschulzdev/lr_messagehub:latest

docker push tobiasschulzdev/nowy_database:latest
docker push tobiasschulzdev/lr_database:latest
docker push tobiasschulzdev/nowy_messagehub:latest
docker push tobiasschulzdev/lr_messagehub:latest

ssh -o StrictHostKeyChecking=no root@mercury.leuchtraketen.cloud 'kubectl rollout restart deploy -n lr_database'
ssh -o StrictHostKeyChecking=no root@mercury.leuchtraketen.cloud 'kubectl rollout restart deploy -n lr_messagehub'

