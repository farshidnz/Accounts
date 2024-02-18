import { App } from "aws-cdk-lib";
import { Template } from "aws-cdk-lib/assertions";
import { DeployStack } from "../lib/deploy-stack";
import { StartsWithMatch } from "./helper";
import { CfnElement } from "aws-cdk-lib";

describe("Should create CDK deployment stack ", () => {
  process.env.AWS_ACCOUNT_ID = "1234567890";
  process.env.AWS_REGION = "ap-southeast-2";
  process.env.ENVIRONMENT_NAME = "test";
  process.env.PROJECT_NAME = "dotnet5tempalte";
  process.env.VERSION = "latest";
  process.env.Environment = "test";
  process.env.MaximumMessageCount = '10';
  process.env.MemberTopicArn = "arn:aws:sns:ap-southeast-2:752830773963:Test";
  process.env.PostgresDbUsernameSsmParam = "/accounts/default/PostgresDbUsername";
  process.env.PostgresDbPasswordSsmParam = "/accounts/default/PostgresDbPassword";
  process.env.ReportSubscriptionTopicArn = "arn:aws:sns:ap-southeast-2:752830773963:TestReport";

  const app = new App();
  // WHEN
  const stack = new DeployStack(app, "testStack", {
    env: {
      account: process.env.AWS_ACCOUNT_ID,
      region: process.env.AWS_REGION,
    },
  });
  const template = Template.fromStack(stack);
  const service = stack.ecsConstruct;

  test("Should ecs fargate service", () => {
    template.hasResourceProperties("AWS::ECS::Service", {
      Cluster: `${process.env.ENVIRONMENT_NAME}-ecs`,
      DeploymentConfiguration: {
        MaximumPercent: 200,
        MinimumHealthyPercent: 50,
      },
      DesiredCount: 1,
      EnableECSManagedTags: false,
      EnableExecuteCommand: true,
      HealthCheckGracePeriodSeconds: 60,
      LaunchType: "FARGATE",
      LoadBalancers: [
        {
          ContainerName: `${process.env.ENVIRONMENT_NAME}-dotnet5tempalte-container`,
          ContainerPort: 80,
          TargetGroupArn: {
            Ref: new StartsWithMatch(
              "TargetGroupArn",
              `${process.env.ENVIRONMENT_NAME}ecs${process.env.ENVIRONMENT_NAME}dotnet5tempaltetg`
            ),
          },
        },
      ],
      NetworkConfiguration: {
        AwsvpcConfiguration: {
          AssignPublicIp: "DISABLED",
          SecurityGroups: [
            {
              "Fn::GetAtt": [
                new StartsWithMatch(
                  "SecurityGroups",
                  `${process.env.ENVIRONMENT_NAME}ecs${process.env.ENVIRONMENT_NAME}dotnet5tempaltesg`
                ),
                "GroupId",
              ],
            },
          ],
          Subnets: [
            new StartsWithMatch("Subnet1", "p-"),
            new StartsWithMatch("Subnet2", "p-"),
          ],
        },
      },
      ServiceName: `${process.env.ENVIRONMENT_NAME}-dotnet5tempalte-ecsService`,
      TaskDefinition: {
        Ref: new StartsWithMatch(
          "TaskDefinition",
          `${process.env.ENVIRONMENT_NAME}ecs${process.env.ENVIRONMENT_NAME}dotnet5tempalteecsTD`
        ),
      },
    });
  });

  test("Should Subscribe to Member Topic", () => {
    template.hasResourceProperties("AWS::SNS::Subscription", {
      Protocol: "sqs",
      Endpoint: {
        "Fn::GetAtt": [
          new StartsWithMatch('',
            `${process.env.ENVIRONMENT_NAME}MemberTopicSubscription`
          ),
          "Arn"
       ]
      },
      TopicArn:
         "arn:aws:sns:ap-southeast-2:752830773963:Test",
      FilterPolicy: {
          EventType: ["MemberSignedUp", "MemberSignedIn", "MemberCredentialChanged", "PasswordChanged"],
      },
      RawMessageDelivery: true,
      Region: "ap-southeast-2"
    });
  });

  test("Should Subscribe to ReportSubscription Topic", () => {
    template.hasResourceProperties("AWS::SNS::Subscription", {
      Protocol: "sqs",
      Endpoint: {
        "Fn::GetAtt": [
          new StartsWithMatch('',
            `${process.env.ENVIRONMENT_NAME}ReportSubscriptionTopicSubscription`
          ),
          "Arn"
       ]
      },
      TopicArn:
         "arn:aws:sns:ap-southeast-2:752830773963:TestReport",
      FilterPolicy: {
          EventType: ["ReportSubscriptionMessageEvent"],
          Context: [
              "MemberUpdate",
              "CognitoMemberUpdate",
              "PersonUpdate"
            ]
      },
      RawMessageDelivery: true,
      Region: "ap-southeast-2"
    });
  });

  test("Should create IAM managed policy", () => {
    template.hasResourceProperties("AWS::IAM::Policy", {
      PolicyDocument: {
        Statement: [
          {
            Action: [
              "ssmmessages:CreateControlChannel",
              "ssmmessages:CreateDataChannel",
              "ssmmessages:OpenControlChannel",
              "ssmmessages:OpenDataChannel"
            ],
            Effect: "Allow",
            Resource: "*"
          },
          {
            Action: [
              "ecr:BatchCheckLayerAvailability",
              "ecr:GetDownloadUrlForLayer",
              "ecr:BatchGetImage"
            ],
            Effect: "Allow",
            Resource: "arn:aws:ecr:ap-southeast-2:811583718322:811583718322.dkr.ecr.ap-southeast-2.amazonaws.com/dotnet5tempalte"
          },
          {
            Action: "ecr:GetAuthorizationToken",
            Effect: "Allow",
            Resource: "*"
          },
          {
            Action: [
              "logs:CreateLogStream",
              "logs:PutLogEvents"
            ],
            Effect: "Allow",
            Resource: {
              "Fn::GetAtt": [
                "testecstestdotnet5tempaltelogGroup784546F8",
                "Arn"
              ]
            }
          },
          {
            "Action": [
              "ssm:DescribeParameters",
              "ssm:GetParameters",
              "ssm:GetParameter",
              "ssm:GetParameterHistory"
            ],
            "Effect": "Allow",
            "Resource": {
              "Fn::Join": [
                "",
                [
                  "arn:",
                  {
                    "Ref": "AWS::Partition"
                  },
                  ":ssm:ap-southeast-2:1234567890:parameter/accounts/default/PostgresDbUsername"
                ]
              ]
            }
          },
          {
            "Action": [
              "ssm:DescribeParameters",
              "ssm:GetParameters",
              "ssm:GetParameter",
              "ssm:GetParameterHistory"
            ],
            "Effect": "Allow",
            "Resource": {
              "Fn::Join": [
                "",
                [
                  "arn:",
                  {
                    "Ref": "AWS::Partition"
                  },
                  ":ssm:ap-southeast-2:1234567890:parameter/accounts/default/PostgresDbPassword"
                ]
              ]
            }
          }
        ],
        Version: "2012-10-17",
      },
    });
  });
});
