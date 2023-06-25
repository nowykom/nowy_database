#!/bin/bash

docker image build -f Nowy.Database.Web/Dockerfile .. -t tobiasschulzdev/nowy_database:latest

docker push tobiasschulzdev/nowy_database:latest

ssh -o StrictHostKeyChecking=no root@mercury.leuchtraketen.cloud 'kubectl rollout restart deployment/trustitution-admin-web'
ssh -o StrictHostKeyChecking=no root@mercury.leuchtraketen.cloud 'kubectl rollout restart deployment/trustitution-transfer-ladies'

