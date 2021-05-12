#! /bin/bash

docker build -t cleancadet/gateway \
    --file Dockerfile \
    --target gatewayWithFront \
    --build-arg 'GATEWAY_URL=http://127.0.0.1:8080/smart-tutor/api/' \
    --build-arg 'KEYCLOAK_AUTH=http://127.0.0.1:8080/keycloak/auth' \
    --build-arg 'KEYCLOAK_ON=true' \
    --build-arg 'REALM=master' \
    --build-arg 'AUDIENCE=demo-app' \
    --build-arg SRC_URL=https://github.com/Clean-CaDET/platform-tutor-ui-web/archive/refs/heads/keycloak-login-deploy.tar.gz \
    --no-cache files