pipeline { 
  agent{
     label 'mono3'
  }
  options {
    buildDiscarder(logRotator(artifactDaysToKeepStr: '', artifactNumToKeepStr: '', daysToKeepStr: '', numToKeepStr: '5'))
    disableConcurrentBuilds()
  }
  stages {
    stage('Checkout') {
      steps {
        container('default') {
            checkout([
            $class: 'GitSCM', branches: [[name: 'feature/MTM-35567/migrate_jenkinsfile']],
            extensions: [[$class: 'CleanCheckout']],
            userRemoteConfigs: [[url: 'git@bitbucket.org:m2m/cumulocity-clients-cs.git',credentialsId:'jenkins-master']] ])
        }
    }
    stage('Build') {
      steps {
        container('dotnet-sdk-3-1') {
          sshagent(['jenkins-master']){
            sh 'cd DeviceSDK/MQTT/ && chmod +x ./build.sh && pwd && ./build.sh -target=Test'
           }
        }
     }
  }
}

