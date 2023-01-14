node('testing-node') {
    properties([
        disableConcurrentBuilds(),
    ])

    stage("Test test") {
        checkout scm
    }

    String testScriptParameters = '--logger=trx --no-restore --no-build --results-directory=./results'
    String postresUserPassword = 'postgres'

    Map<String, String> containerEnvVars = [

        // Postgres
        'POSTGRES_CONNECTION_RETRIES': 5,
        'POSTGRES_USER': postresUserPassword,
        'POSTGRES_PASSWORD': postresUserPassword,
        'POSTGRES_DATABASE': "template1",

        'ConnectionStrings__DefaultConnection': "User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=postgres;Pooling=true;Include Error Detail=true;Log Parameters=true;",
        'Hibernate__IsShowSql': "false"
    ]

    runStage(Stage.UPDATE_GIT_STATUS) {
        updateGithubCommitStatus('Set PENDING status', 'PENDING')
    }

    preconfigureAndStart(({ networkId ->
        runStage(Stage.CLEAN) {
            // Clean before build
            cleanWs()
        }
    
        runStage(Stage.CHECKOUT) {
            sh """
                git config --global http.postBuffer 2048M
                git config --global http.maxRequestBuffer 1024M
                git config --global core.compression 9
            """
            checkout scm
        }
        
        runStage(Stage.SET_VARS) {
            withCredentials([string(credentialsId: "timevic_testing_clickup_secret_key", variable: 'AUTH_SECRET')]) {
                containerEnvVars.put('Integration__ClickUp__SecurityKey', AUTH_SECRET)
            }

            withCredentials([string(credentialsId: "timevic_testing_redmine_api_key", variable: 'AUTH_SECRET')]) {
                containerEnvVars.put('Integration__Redmine__ApiKey', AUTH_SECRET)
            }

            withCredentials([string(credentialsId: "timevic_testing_redmine_url", variable: 'AUTH_SECRET')]) {
                containerEnvVars.put('Integration__Redmine__Url', AUTH_SECRET)
            }

            withCredentials([string(credentialsId: "timevic_testing_google__storage_project_id", variable: 'AUTH_SECRET')]) {
                containerEnvVars.put('Google__Storage__ProjectId', AUTH_SECRET)
            }

            withCredentials([string(credentialsId: "timevic_testing_google__storage_bucket_name", variable: 'AUTH_SECRET')]) {
                containerEnvVars.put('Google__Storage__BucketName', AUTH_SECRET)
            }
        }

        def testImage = docker.build('timevic-test-image', '--file=./devops/test-github/Dockerfile .')
        String containerEnvVarString = mapToEnvVars(containerEnvVars)
        testImage.inside(containerEnvVarString.concat(" --network=$networkId")) {

            runStage(Stage.ADD_GCLOUD_CREDENTIALS) {
                withCredentials([file(credentialsId: 'timevic_testing_gcloud_credentials', variable: 'FILE')]) {
                    sh 'cp $FILE .credentials/google.json'
                }
            }

            runStage(Stage.BUILD) {
                sh 'echo "{}" > appsettings.Local.json'
                sh 'echo "{}" > TimeTracker.Tests.Integration.Api/appsettings.Local.json'
                sh 'echo "{}" > TimeTracker.Tests.Integration.Business/appsettings.Local.json'
                sh 'echo "{}" > TimeTracker.Migrations/appsettings.Local.json'
                sh 'dotnet build --'
            }

            runStage(Stage.INIT_DB) {
                sh 'psql --version'
                sh 'pg_ctlcluster 15 main start'
                sh 'pg_isready'
                sh "sudo -u postgres psql -c \"ALTER USER postgres PASSWORD '$postresUserPassword';\""
                sh "PGPASSWORD=postgres psql -h localhost --username=$postresUserPassword --dbname=$postresUserPassword -c \"select 1\""
                echo 'Postgre SQL is started'
            }

            runStage(Stage.INIT_REDIS) {
                sh '/usr/bin/redis-server &'
                sh 'until nc -z localhost 6379; do sleep 1; done'
                echo "Redis is started"
                
                sh 'netstat -tulpn | grep LISTEN'
            }

            runStage(Stage.RUN_MIGRATIONS) {
                sh 'dotnet run --no-restore --no-build --project ./TimeTracker.Migrations'
            }

            runStage(Stage.RUN_UNIT_TESTS) {
                sh 'dotnet test --logger trx --verbosity=quiet --results-directory /tmp/test ./TimeTracker.Tests.Unit.Business'
            }
            
            runStage(Stage.RUN_INTEGRATION_TESTS_1) {
                sh 'dotnet test --logger trx --verbosity=quiet --results-directory /tmp/test ./TimeTracker.Tests.Integration.Business'
            }

            runStage(Stage.RUN_INTEGRATION_TESTS_2) {
                sh 'dotnet test --logger trx --verbosity=quiet --results-directory /tmp/test ./TimeTracker.Tests.Integration.Api'
            }
        }

        runStage(Stage.UPDATE_GIT_STATUS) {
            updateGithubCommitStatus('Set SUCCESS status', 'SUCCESS')
        }
    } as Closure<String>))
}

enum Stage {
    UPDATE_GIT_STATUS('Update git status'),
    CLEAN('Clean'),
    CHECKOUT('Checkout'),
    SET_VARS('Set environment vars'),
    ADD_GCLOUD_CREDENTIALS('Add GCloud credentials'),
    BUILD('Build projects'),
    INIT_DB('Init DB'),
    INIT_REDIS('Init Redis'),
    RUN_MIGRATIONS('Run migrations'),
    RUN_UNIT_TESTS('Run unit tests'),
    RUN_INTEGRATION_TESTS_1('Run business logic integration tests'),
    RUN_INTEGRATION_TESTS_2('Run API integration tests'),

//    SAVE_ARTIFACTS('Save artifacts'),

    private final String name;

    private Stage(String s) {
        this.name = s;
    }

    String toString() {
        return this.name;
    }

    static String[] toListOfStrings() {
        def result = []
        for (def stage: values()) {
            result.add(stage.toString())
        }
        return result.reverse()
    }
}

def mapToEnvVars(Map<String, String> list) {
    String result = ''
    list.each {
        result = "$result -e $it.key=\"$it.value\""
    }
    return result
}

def preconfigureAndStart(Closure<String> inner) {
    def networkId = UUID.randomUUID().toString()
    try {
        sh "docker network rm ${networkId}"
    } catch(Exception exception) {
        println exception.getMessage()
    }
    try {
        sh "docker network create ${networkId}"
        inner(networkId)
    } catch(Exception exception) {
        updateGithubCommitStatus(exception.getMessage(), 'ERROR')
        println exception.getMessage()
    } finally {
        sh "docker network rm ${networkId}"
    }
}

def runStage(Stage stageAction, Closure callback) {
    stage(stageAction.toString()) {
        try {
            callback()
        } catch (Exception e) {
            throw new Exception(e.getMessage())
        }
    }
}

def getRepoURL() {
  sh "git config --get remote.origin.url > .git/remote-url"
  return readFile(".git/remote-url").trim()
}

def getCommitSha() {
  sh "git rev-parse HEAD > .git/current-commit"
  return readFile(".git/current-commit").trim()
}

def updateGithubCommitStatus(String message, String state) {
        String commitHash = getCommitSha()
        String repositoryUrl = getRepoURL()

        // workaround https://issues.jenkins-ci.org/browse/JENKINS-38674
        step([
            $class: 'GitHubCommitStatusSetter',
            reposSource: [$class: "ManuallyEnteredRepositorySource", url: repositoryUrl],
            commitShaSource: [$class: "ManuallyEnteredShaSource", sha: commitHash],
            errorHandlers: [[$class: 'ShallowAnyErrorHandler']],
            statusResultSource: [ $class: "ConditionalStatusResultSource", results: [[$class: "AnyBuildResult", message: message, state: state]] ]
        ])
    }
