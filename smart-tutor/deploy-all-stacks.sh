#! /bin/bash

echo "********** DEPLOYING PERSISTENCE STACK **********"
pushd stacks/persistence/scripts/ > /dev/null
./deploy.sh
popd > /dev/null

echo "********** DEPLOYING APPLICATION STACK **********"
pushd stacks/application/scripts/ > /dev/null
./deploy.sh
popd > /dev/null

echo "********** DEPLOYING PUBLIC STACK **********"
pushd stacks/public/scripts/ > /dev/null
./deploy.sh
popd > /dev/null

