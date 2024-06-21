@Library('common')
import com.shared.jenkins.docker.DockerHelper
import com.shared.jenkins.docker.DockerContainer

def environmentKey = params.ENVIRONMENT.toLowerCase()

def dockerHelper = new DockerHelper(this)
public Map<String, String> envVariables = new HashMap<String, String>()

def mainContainer = new DockerContainer(
    name: "timevic-main-${environmentKey}",
    dockerFile: 'devops/publish_native/common/Dockerfile',
);

def migrationContainer = new DockerContainer(
    name: "timevic-main-${environmentKey}",
    dockerFile: 'devops/publish_native/common/Dockerfile',
    isRunAlways: false,
    isRunInBackground: false,
);
def webAppContainer = new DockerContainer(
    name: "timevic-web-${environmentKey}",
    dockerFile: 'devops/publish_native/web/Dockerfile',
);

def repositoryUrl = scm.userRemoteConfigs[0].url;

properties([
    parameters([
        // https://plugins.jenkins.io/git-parameter/
        gitParameter (name: 'GIT_TAG', type: 'PT_TAG', sortMode: 'DESCENDING_SMART', selectedValue: 'NONE'),
        choice(name: 'ENVIRONMENT', choices: ['Development', 'Production'], description: 'Select environment to deploy'),
    ]),
    disableConcurrentBuilds()
])

node('abedor-mainframe-web') {

    stage('Show deployment parameters') {
        echo "Repository: ${repositoryUrl}"
        echo "Environment: ${params.ENVIRONMENT}"
        echo "Tag: ${params.GIT_TAG}"
    }

    if (!params.GIT_TAG?.trim())
    {
        stage('Switch to GIT tag') {
            git branch: "${params.BRANCH}", url: repositoryUrl
        }    
    }

    stage('CleanUp Docker') {
        sh """
            docker image prune -f
        """
    }

    stage('Checkout') {
        cleanWs()
        sh """
            git config --global http.postBuffer 2048M
            git config --global http.maxRequestBuffer 1024M
            git config --global core.compression 0
        """
        checkout scm
    }

    stage('Set environment vars') {
        // Redis
        envVariables.put('Redis__Server', '10.10.0.2:6379')
        envVariables.put('Serilog__IsSendEmailIfError', 'false')
        envVariables.put('Serilog__MinimumLevel__Default', 'Debug')

        def dbName = ''
        if (params.ENVIRONMENT == 'Production')
        {
            envVariables.put('App__FrontendUrl', 'https://timevic.com')
            dbName = 'timevic'
        }
        else if (params.ENVIRONMENT == 'Development')
        {
            envVariables.put('App__FrontendUrl', 'https://dev.timevic.com')
            dbName = 'timevic_dev'
        }

        // Common
        withCredentials([
                usernamePassword(credentialsId: "timevic_production_smtp_credentials", usernameVariable: 'USER_NAME', passwordVariable: 'PASSWORD')
        ]) {
            envVariables.put('Smtp__UserName', USER_NAME)
            envVariables.put('Smtp__Password', PASSWORD)
        }
        withCredentials([string(credentialsId: "timevic_production_recaptcha_secret", variable: 'AUTH_SECRET')]) {
            envVariables.put('ReCaptcha__Secret', AUTH_SECRET)
        }

        withCredentials([
                usernamePassword(credentialsId: "timevic_${environmentKey}_db_credentials", usernameVariable: 'USER_NAME', passwordVariable: 'PASSWORD')
        ]) {
            envVariables.put(
                'ConnectionStrings__DefaultConnection',
                "User ID=${USER_NAME};Password=${PASSWORD};Host=192.168.99.8;Port=5433;Database=${dbName};Pooling=true;"
            )
        }
        withCredentials([string(credentialsId: "timevic_${environmentKey}_user_jwt", variable: 'AUTH_SECRET')]) {
            envVariables.put('App__Auth__SymmetricSecurityKey', AUTH_SECRET)
        }
        
        // withCredentials([string(credentialsId: "timevic_${environmentKey}_google__storage_project_id", variable: 'AUTH_SECRET')]) {
        //     envVariables.put('Google__Storage__ProjectId', AUTH_SECRET)
        // }

        // withCredentials([string(credentialsId: "timevic_${environmentKey}_google__storage_bucket_name", variable: 'AUTH_SECRET')]) {
        //     envVariables.put('Google__Storage__BucketName', AUTH_SECRET)
        // }
        
        withCredentials([
            usernamePassword(credentialsId: "timevic_${environmentKey}_aws_s3_credentials", usernameVariable: 'USER_NAME', passwordVariable: 'PASSWORD')
        ]) {
            envVariables.put('AWS__S3__AccessKey', USER_NAME)
            envVariables.put('AWS__S3__SecretKey', PASSWORD)
        }
        envVariables.put('AWS__S3__BucketName', 'timevic-${environmentKey}')
    }

    stage('Build main image') {
        withCredentials([file(credentialsId: 'timevic_production_gcloud_credentials', variable: 'FILE')]) {
            sh 'cp $FILE .credentials/google.json'
        }
        dockerHelper.buildContainer(mainContainer)
    }

    stage('Build web image') {
        dockerHelper.buildContainer(webAppContainer)
    }

    stage('Stop containers') {
        dockerHelper.stopContainer(webAppContainer)

        mainContainer.tagName = "timevic-api-${environmentKey}";
        dockerHelper.stopContainer(mainContainer)
    
        mainContainer.tagName = "timevic-worker-${environmentKey}";
        dockerHelper.stopContainer(mainContainer)
    }

    stage('Run migrations') {
        dockerHelper.stopContainer(migrationContainer)
            
        migrationContainer.envVariables = envVariables.clone()
        migrationContainer.envVariables.put('PROJECT_DIR', 'TimeTracker.Migrations')
        dockerHelper.runContainer(migrationContainer)
    }

    stage('Run common API') {
        mainContainer.tagName = "timevic-api-${environmentKey}";
         if (params.ENVIRONMENT == 'Production')
        {
            mainContainer.port = '6200:80';
        }
        else if (params.ENVIRONMENT == 'Development')
        {
            mainContainer.port = '8215:80';
        }
        
        mainContainer.envVariables = envVariables.clone()
        mainContainer.envVariables.put('PROJECT_DIR', 'TimeTracker.Api')
        dockerHelper.runContainer(mainContainer)
    }

    stage('Run worker') {
        mainContainer.tagName = "timevic-worker-${environmentKey}";
        mainContainer.port = '';
        
        mainContainer.envVariables = envVariables.clone()
        mainContainer.envVariables.put('PROJECT_DIR', 'TimeTracker.WorkerServices')
        dockerHelper.runContainer(mainContainer)
    }

    stage('Run web app') {
        if (params.ENVIRONMENT == 'Production')
        {
            webAppContainer.port = '6201:80';
        }
        else if (params.ENVIRONMENT == 'Development')
        {
            webAppContainer.port = '8216:80';
        }
        dockerHelper.runContainer(webAppContainer)
    }

    stage("Clean workspace") {
        cleanWs()
    }
}
