import { DomainEventFilter, DomainTopicSubscriptionConstruct, EcsConstruct, getEnv, getResourceName, ServiceVisibility, applyMetaTags } from "@cashrewards/cdk-lib";
import { Fn, Duration, Stack, StackProps } from "aws-cdk-lib";
import { Effect, ManagedPolicy, PolicyDocument, PolicyStatement } from "aws-cdk-lib/aws-iam";
import { SubscriptionFilter, Topic } from "aws-cdk-lib/aws-sns";
import { Construct } from "constructs";

export class DeployStack extends Stack {
  public ecsConstruct: EcsConstruct;
  constructor(scope: Construct, id: string, props?: StackProps) {
    super(scope, id, props);

    const PostgreSQLSecretName = this.getExportName("PostgresDbClusterSecretName");
    const postgreSQLsecretName = Fn.importValue(PostgreSQLSecretName);

    this.ecsConstruct = new EcsConstruct(this, getResourceName("ecs"), {
      environmentName: getEnv("ENVIRONMENT_NAME"),
      serviceName: getEnv("PROJECT_NAME"),
      pathPattern: "api/accounts",
      healthCheckPath: "api/accounts/health-check",
      visibility: ServiceVisibility.PUBLIC,
      listenerRulePriority: 100,
      imageTag: getEnv("VERSION"),
      scalingRule: {
        cpuScaling: {
          targetUtilizationPercent: 40,
          scaleInCooldown: 60,
          scaleOutCooldown: 60,
          alarm: {
            enableSlackAlert: true,
          },
        },
        memoryScaling: {
          targetUtilizationPercent: 40,
          scaleInCooldown: 60,
          scaleOutCooldown: 60,
          alarm: {
            enableSlackAlert: true,
            threshold: 25,
          },
        },
      },
      environment: {
        Environment: getEnv("Environment"),
        ServiceName: getEnv("PROJECT_NAME"),
        AccountDomainPIIKeyArn : getEnv('AccountDomainPIIKey'),
        LOG_LEVEL: getEnv("LOG_LEVEL"),
        EnableDevops4: getEnv("EnableDevops4"),
        SwitchToPostgresDB: getEnv("SwitchToPostgresDB"),
        DbSecretName: postgreSQLsecretName,
        ConfigureDatabase: getEnv("ConfigureDatabase"),
        AWS__MainSiteApiKeyName: getEnv("MainSiteApiKeyName")
      },
      secrets:{
        PostgresDbUsername:getEnv("PostgresDbUsernameSsmParam"),
        PostgresDbPassword:getEnv("PostgresDbPasswordSsmParam")
      }
    });
    
    const topic = Topic.fromTopicArn(
      this,
      "MemberTopicArn",
      getEnv("MemberTopicArn")
    );
    topic.grantPublish(this.ecsConstruct.taskRole);
    this.ecsConstruct.taskRole.addToPrincipalPolicy(new PolicyStatement({
       actions: [
        "SNS:ListTopics",
        "secretsmanager:GetSecretValue",
        "ssm:GetParameter",
        "ssm:GetParameters",
        "ssm:GetParametersByPath",
        "kms:Decrypt",
        "kms:Encrypt",
        "kms:DescribeKey"
       ],
       resources: [ "*" ]
    }));

    let domainTopicSub = new DomainTopicSubscriptionConstruct(this, getResourceName("MemberTopicSubscription"), {
      domain: "Member",
      environmentName: getEnv("ENVIRONMENT_NAME"),
      serviceName: getEnv("PROJECT_NAME"),
      maximumMessageCount: +getEnv('MaximumMessageCount'),
      filterPolicy: DomainEventFilter.ByEventType([
        "MemberSignedUp",
        "MemberSignedIn",
        "MemberCredentialChanged",
        "PasswordChanged"
      ]), // Filter for the events you're interested in, if blank, you'll get everything
      maxReceiveCount: 10, // move event to DLQ after 10 unsuccessful retries
      retentionPeriod: Duration.days(14) // keep the events in SQS upto 14 days
    });
    
      domainTopicSub.messageQueue.grantConsumeMessages(this.ecsConstruct.taskRole);
      domainTopicSub.messageQueue.grantPurge(this.ecsConstruct.taskRole);

      this.ecsConstruct.node.addDependency(domainTopicSub);

      // Subscribe to ReportSubscription topic to sync data from ShopGo DB to "copied" tables in dto schema
      let reportSubDomainTopicSub = new DomainTopicSubscriptionConstruct(this, getResourceName("ReportSubscriptionTopicSubscription"), {
        domain: "ReportSubscription",
        environmentName: getEnv("ENVIRONMENT_NAME"),
        serviceName: getEnv("PROJECT_NAME"),
        filterPolicy: {
          EventType: SubscriptionFilter.stringFilter({
              allowlist: ["ReportSubscriptionMessageEvent"],
          }),
          Context: SubscriptionFilter.stringFilter({
              allowlist: ["MemberUpdate", "CognitoMemberUpdate", "PersonUpdate"], // only subscribe to member related table updates
          })
        },
        maxReceiveCount: 10, // move event to DLQ after 10 unsuccessful retries
        retentionPeriod: Duration.days(14) // keep the events in SQS upto 14 days
      });

      reportSubDomainTopicSub.messageQueue.grantConsumeMessages(this.ecsConstruct.taskRole);
      reportSubDomainTopicSub.messageQueue.grantPurge(this.ecsConstruct.taskRole);

      this.ecsConstruct.node.addDependency(reportSubDomainTopicSub);
  
      applyMetaTags(this, { 'Team': 'Accounts', 'Application': 'Accounts' });
  }

  get ecsConstructObj() {
    return this.ecsConstruct;
  }
    private getExportName(name: string) {
        return `${getEnv("ENVIRONMENT_NAME")}-${name}`;
    }

}
