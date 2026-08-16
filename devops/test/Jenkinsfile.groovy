node('build-node') {
    properties([
        disableConcurrentBuilds(),
//         gitLabConnection('gitlab_lampego'),
    ])

    String testScriptParameters = '--logger=trx --no-restore --no-build --results-directory=./results'
    String postresUserPassword = 'postgres'

    String dbNameApi = 'test_api'
    String dbNameBusiness = 'test_business'

    String dbConnectionApi = "User ID=postgres;Password=${postresUserPassword};Host=localhost;Port=5432;Database=${dbNameApi};Pooling=true;Include Error Detail=true;Log Parameters=true;"
    String dbConnectionBusiness = "User ID=postgres;Password=${postresUserPassword};Host=localhost;Port=5432;Database=${dbNameBusiness};Pooling=true;Include Error Detail=true;Log Parameters=true;"

    Map<String, String> containerEnvVars = [
        // Postgres
        'POSTGRES_CONNECTION_RETRIES': 5,
        'POSTGRES_USER': postresUserPassword,
        'POSTGRES_PASSWORD': postresUserPassword,
        'POSTGRES_DATABASE': "template1",

        // Redis
        'Redis__Server': "localhost:6379",

        'Hibernate__IsShowSql': "false"
    ]

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

            withCredentials([string(credentialsId: "timevic_testing_google__storage_project_id", variable: 'AUTH_SECRET')]) {
                containerEnvVars.put('Google__Storage__ProjectId', AUTH_SECRET)
            }

            withCredentials([string(credentialsId: "timevic_testing_google__storage_bucket_name", variable: 'AUTH_SECRET')]) {
                containerEnvVars.put('Google__Storage__BucketName', AUTH_SECRET)
            }
            
            withCredentials([
                usernamePassword(credentialsId: "timevic_testing_aws_s3_credentials", usernameVariable: 'USER_NAME', passwordVariable: 'PASSWORD')
            ]) {
                containerEnvVars.put('AWS__S3__AccessKey', USER_NAME)
                containerEnvVars.put('AWS__S3__SecretKey', PASSWORD)
            }

            withCredentials([
                usernamePassword(credentialsId: "timevic_testing_garage_credentials", usernameVariable: 'USER_NAME', passwordVariable: 'PASSWORD')
            ]) {
                containerEnvVars.put('Garage__AccessKey', USER_NAME)
                containerEnvVars.put('Garage__SecretKey', PASSWORD)
            }
            withCredentials([string(credentialsId: "timevic_garage_url", variable: 'AUTH_SECRET')]) {
                containerEnvVars.put('Garage__Url', AUTH_SECRET)
            }
            withCredentials([string(credentialsId: "timevic_testing_garage_bucket_name", variable: 'AUTH_SECRET')]) {
                containerEnvVars.put('AWS__S3__BucketName', AUTH_SECRET)
            }
        }

        def testImage = docker.build('timevic-test-image', '--file=./devops/test/Dockerfile .')
        String containerEnvVarString = mapToEnvVars(containerEnvVars)
        testImage.inside(containerEnvVarString.concat(" --network=$networkId")) {

            runStage(Stage.ADD_GCLOUD_CREDENTIALS) {
                withCredentials([file(credentialsId: 'timevic_testing_gcloud_credentials', variable: 'FILE')]) {
                    sh 'cp $FILE .credentials/google.json'
                }
            }

            runStage(Stage.BUILD) {
                sh 'echo "{}" > appsettings.Local.json'
                sh 'echo "{}" > TimeTracker.Api/appsettings.Local.json'
                sh 'echo "{}" > TimeTracker.Tests.Integration.Api/appsettings.Local.json'
                sh 'echo "{}" > TimeTracker.Migrations/appsettings.Local.json'
                sh 'echo "{}" > TimeTracker.Tests.Integration.Business/appsettings.Local.json'
                sh 'echo "{}" > TimeTracker.Tests.Unit.Business/appsettings.Local.json'
                sh 'echo "{}" > TimeTracker.WorkerServices/appsettings.Local.json'
                sh '''
                    dotnet build ./TimeTracker.Migrations/TimeTracker.Migrations.csproj
                    dotnet build ./TimeTracker.Tests.Integration.Api/TimeTracker.Tests.Integration.Api.csproj
                    dotnet build ./TimeTracker.Tests.Integration.Business/TimeTracker.Tests.Integration.Business.csproj
                    dotnet build ./TimeTracker.Tests.Unit.Business/TimeTracker.Tests.Unit.Business.csproj
                '''
            }

            runStage(Stage.INIT_DB) {
                sh 'pg_ctlcluster 16 main start'
                sh 'pg_isready'
                sh "sudo -u postgres psql -c \"ALTER USER postgres PASSWORD '$postresUserPassword';\""
                sh "PGPASSWORD=$postresUserPassword psql -h localhost --username=postgres --dbname=postgres -c \"select 1\""
                sh "sudo -u postgres psql -c \"DROP DATABASE IF EXISTS ${dbNameApi};\""
                sh "sudo -u postgres psql -c \"CREATE DATABASE ${dbNameApi};\""
                sh "sudo -u postgres psql -c \"DROP DATABASE IF EXISTS ${dbNameBusiness};\""
                sh "sudo -u postgres psql -c \"CREATE DATABASE ${dbNameBusiness};\""
                echo 'Postgre SQL is started and databases are created'
            }

            runStage(Stage.INIT_REDIS) {
                sh '/usr/bin/redis-server &'
                sh 'until nc -z localhost 6379; do sleep 1; done'
                echo "Redis is started"
                
                sh 'netstat -tulpn | grep LISTEN'
            }

            runStage(Stage.RUN_MIGRATIONS) {
                sh """
                    ConnectionStrings__DefaultConnection="${dbConnectionApi}" dotnet run --no-restore --no-build --project ./TimeTracker.Migrations
                    ConnectionStrings__DefaultConnection="${dbConnectionBusiness}" dotnet run --no-restore --no-build --project ./TimeTracker.Migrations
                """
            }

            parallel(
                'API Integration Tests': {
                    runStage(Stage.RUN_API_UNIT_TESTS) {
                        sh "ConnectionStrings__DefaultConnection=\"${dbConnectionApi}\" dotnet test ${testScriptParameters} --verbosity=normal ./TimeTracker.Tests.Integration.Api"
                    }
                },
                'Business Integration Tests': {
                    runStage(Stage.RUN_BUSINESS_LOGIC_UNIT_TESTS) {
                        sh "ConnectionStrings__DefaultConnection=\"${dbConnectionBusiness}\" dotnet test ${testScriptParameters} --verbosity=normal ./TimeTracker.Tests.Integration.Business"
                    }
                },
                'Business Unit Tests': {
                    runStage(Stage.RUN_BUSINESS_UNIT_TESTS) {
                        sh "dotnet test ${testScriptParameters} --verbosity=normal ./TimeTracker.Tests.Unit.Business"
                    }
                }
            )
        }
    } as Closure<String>))
}

enum Stage {
    CLEAN('Clean'),
    CHECKOUT('Checkout'),
    ADD_GCLOUD_CREDENTIALS('Add GCloud credentials'),
    BUILD('Build projects'),
    SET_VARS('Set environment vars'),
    INIT_DB('Init DB'),
    INIT_REDIS('Init Redis'),
    RUN_MIGRATIONS('Run migrations'),
    RUN_API_UNIT_TESTS('Run API integration tests'),
    RUN_BUSINESS_LOGIC_UNIT_TESTS('Run Business integration tests'),
    RUN_BUSINESS_UNIT_TESTS('Run Business unit tests'),

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
        def code = sh(script: "docker network rm ${networkId}", returnStatus: true)
        if (code == 1) {
            echo "Testing netowrk not found. Skip removing..."
        }
    } catch(Exception exception) {
        println exception.getMessage()
    }
    try {
        sh "docker network create ${networkId}"
//         gitlabBuilds(builds: Stage.toListOfStrings()) {
//             inner.call(networkId)
//         }
        inner.call(networkId)
    } finally {
        def code = sh(script: "docker network rm ${networkId}", returnStatus: true)
        if (code == 1) {
            echo "Network was not removed..."
        }
    }
}

def runStage(Stage stageAction, Closure callback) {
    stage(stageAction.toString()) {
        try {
//             updateGitlabCommitStatus name: stageAction.toString(), state: 'running'
            callback()
//             updateGitlabCommitStatus name: stageAction.toString(), state: 'success'
        } catch (Exception e) {
//             updateGitlabCommitStatus name: stageAction.toString(), state: 'failed'
            throw new Exception(e.getMessage())
        }
    }
}
