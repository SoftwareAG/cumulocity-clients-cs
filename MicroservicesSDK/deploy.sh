#!/bin/sh
deploy() {

YUM_USR=hudson
YUM_USR_KEY=~/.ssh/id_rsa
YUM_SRV=yum.cumulocity.com
YUM_DEST_DIR=/var/www/resources/cssdk/releases

for var in "$@"
do
    echo "deploy $var to $YUM_SRV:${YUM_DEST_DIR}"
    scp -Cr -i ${YUM_USR_KEY} $var ${YUM_USR}@${YUM_SRV}:${YUM_DEST_DIR}
done

ssh -i ${YUM_USR_KEY} ${YUM_USR}@${YUM_SRV} '

win_scripts_latest="microservicesdk-win-dev-latest.zip"
lin_scripts_latest="microservicesdk-lin-dev-latest.zip"

cd /var/www/resources/cssdk/releases

if [ -f $win_scripts_latest ] ; then
        echo remove_win_scripts
        rm  $win_scripts_latest
fi

win_scripts=$(ls -v | grep win-dev | tail -n 1)

if [ -z "$win_scripts" ]
then
      echo "\$win_scripts is empty"
else
      echo "\$win_scripts is NOT empty"
      echo $win_scripts
      cp $win_scripts microservicesdk-win-dev-latest.zip
fi

if [ -f $lin_scripts_latest ] ; then
        echo remove_lin_scripts
        rm  $lin_scripts_latest
fi

lin_scripts=$(ls -v | grep lin-dev | tail -n 1)

if [ -z "$lin_scripts" ]
then
      echo "\$lin_scripts is empty"
else
      echo "\$lin_scripts is NOT empty"
      echo $lin_scripts
      cp $lin_scripts microservicesdk-lin-dev-latest.zip
fi
'

}
pwd
deploy publish/*.nupkg
deploy publish/*.zip