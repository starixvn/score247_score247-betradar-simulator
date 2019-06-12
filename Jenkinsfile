@Library("JenkinsPipelineLibrary") _

pipeline{
    agent any

    options{
        buildDiscarder(logRotator(numToKeepStr: '5'))    
        disableConcurrentBuilds()
    }

    triggers{
        pollSCM("H/30 * * * *")       
    }

    stages{
        stage('Build') {
            steps {        
                script{
                    pipelineLib.beginSonarQubeForMsBuild("LiveScores.API", "LiveScores / Live Scores API", "/d:sonar.cs.opencover.reportsPaths=\"${WORKSPACE}\\CoverageReports\\*.xml\" /d:sonar.cs.vstest.reportsPaths=\"${WORKSPACE}\\TestResults\\*.trx\"")

                    pipelineLib.restoreNuget()

                    pipelineLib.msBuild("LiveScore.Backend.sln")
                }
            }
        }
          
        stage("C# Unit Test"){
            steps{
                script{
                    pipelineLib.xUnitForNetCore()
                }
            }
        }

        stage("SonarQube Analysis"){
            steps{       
                script{
                    pipelineLib.endSonarQubeForMsBuild()
                }
            }
            
            post{
                always{
                    script{
                        if(manager.logContains(".*Quality gate status.*")){              
                            pipelineLib.generateSonarQubeReport("LiveScores.API")
                        }
                    }

                    archiveArtifacts "*.xml,*.email"

                    step([$class: 'ACIPluginPublisher', name: '*.xml', shownOnProjectPage: false])                                       
                    
                    mstest failOnError: false
                }
            }
        }
    }
    post{
        unsuccessful{
            emailext body: '$DEFAULT_CONTENT', subject: '$DEFAULT_SUBJECT', to: 'ricky.nguyen@starixsoft.com, vivian.nguyen@starixsoft.com, harrison.nguyen@starixsoft.com, anders.le@starixsoft.com'
        }
    }
}