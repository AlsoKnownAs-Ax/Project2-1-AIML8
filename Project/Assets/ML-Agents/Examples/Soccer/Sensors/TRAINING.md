# How to train

cd: in the root project

`conda activate [env name]`

`mlagents-learn config/poca/SoccerTwos.yaml --train`

### Adding a run id

`mlagents-learn config/poca/SoccerTwos.yaml --run-id=[run id name] --train`

### Resuming a training session

`mlagents-learn config/poca/SoccerTwos.yaml --resume`

### Override a training session

`mlagents-learn config/poca/SoccerTwos.yaml --force`

### How to see results:

`tensorboard --logdir results`
