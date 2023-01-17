@Library('common')
import com.shared.jenkins.docker.DockerHelper
import com.shared.jenkins.docker.DockerContainer

def dockerHelper = new DockerHelper(this)
public Map<String, String> envVariables = new HashMap<String, String>()

def mainContainer = new DockerContainer(
    name: 'timevic-main-production',
    dockerFile: 'devops/publish_native/common/Dockerfile',
);

def migrationContainer = new DockerContainer(
    name: 'timevic-main-production',
    dockerFile: 'devops/publish_native/common/Dockerfile',
    isRunAlways: false,
    isRunInBackground: false,
);
def webAppContainer = new DockerContainer(
    name: 'timevic-web-production',
    dockerFile: 'devops/publish_native/web/Dockerfile',
);

properties([
    disableConcurrentBuilds()
])

node('abedor-mainframe-web-2') {
    env.ENVIRONMENT = "Development"

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

    stage('Build main image') {
        withCredentials([file(credentialsId: 'timevic_production_gcloud_credentials', variable: 'FILE')]) {
            sh 'cp $FILE .credentials/google.json'
        }
        dockerHelper.buildContainer(mainContainer)
    }

    stage('Build web image') {
        dockerHelper.buildContainer(webAppContainer)
    }

    stage('Set environment vars') {
        envVariables.put('App__FrontendUrl', 'https://timevic.com')
        envVariables.put('Serilog__IsSendEmailIfError', 'false')
        envVariables.put('Serilog__MinimumLevel__Default', 'Debug')

        // Redis
        envVariables.put('Redis__Server', '10.10.0.2:6379')
    
        withCredentials([
                usernamePassword(credentialsId: "timevic_production_db_credentials", usernameVariable: 'USER_NAME', passwordVariable: 'PASSWORD')
        ]) {
            envVariables.put(
                'ConnectionStrings__DefaultConnection',
                "User ID=${USER_NAME};Password=${PASSWORD};Host=192.168.99.8;Port=5433;Database=timevic;Pooling=true;"
            )
        }
        withCredentials([
                usernamePassword(credentialsId: "timevic_production_smtp_credentials", usernameVariable: 'USER_NAME', passwordVariable: 'PASSWORD')
        ]) {
            envVariables.put('Smtp__UserName', USER_NAME)
            envVariables.put('Smtp__Password', PASSWORD)
        }
        withCredentials([string(credentialsId: "timevic_production_user_jwt", variable: 'AUTH_SECRET')]) {
            envVariables.put('App__Auth__SymmetricSecurityKey', AUTH_SECRET)
        }
        withCredentials([string(credentialsId: "timevic_production_recaptcha_secret", variable: 'AUTH_SECRET')]) {
            envVariables.put('ReCaptcha__Secret', AUTH_SECRET)
        }
        withCredentials([string(credentialsId: "timevic_production_google__storage_project_id", variable: 'AUTH_SECRET')]) {
            envVariables.put('Google__Storage__ProjectId', AUTH_SECRET)
        }

        withCredentials([string(credentialsId: "timevic_production_google__storage_bucket_name", variable: 'AUTH_SECRET')]) {
            envVariables.put('Google__Storage__BucketName', AUTH_SECRET)
        }
    }

    stage('Stop containers') {
        dockerHelper.stopContainer(webAppContainer)

        mainContainer.tagName = 'timevic-api';
        dockerHelper.stopContainer(mainContainer)
    
        mainContainer.tagName = 'timevic-worker';
        dockerHelper.stopContainer(mainContainer)
    }

    stage('Run migrations') {
        dockerHelper.stopContainer(migrationContainer)
            
        migrationContainer.envVariables = envVariables.clone()
        migrationContainer.envVariables.put('PROJECT_DIR', 'TimeTracker.Migrations')
        dockerHelper.runContainer(migrationContainer)
    }

    stage('Run common API') {
        mainContainer.tagName = 'timevic-api';
        mainContainer.port = '6200:80';
        
        mainContainer.envVariables = envVariables.clone()
        mainContainer.envVariables.put('PROJECT_DIR', 'TimeTracker.Api')
        dockerHelper.runContainer(mainContainer)
    }

    stage('Run worker') {
        mainContainer.tagName = 'timevic-worker';
        mainContainer.port = '';
        
        mainContainer.envVariables = envVariables.clone()
        mainContainer.envVariables.put('PROJECT_DIR', 'TimeTracker.WorkerServices')
        dockerHelper.runContainer(mainContainer)
    }

    stage('Run web app') {
        webAppContainer.port = '6201:80';
        dockerHelper.runContainer(webAppContainer)
    }

    runStage("Clean workspace") {
        cleanWs()
    }
}
