
import json
import urllib.parse
import boto3
import botocore
import logging

print('Loading function')

s3 = boto3.client('s3')
rekognition = boto3.client('rekognition')

logger = logging.getLogger()
logger.setLevel(logging.INFO)

def lambda_handler(event, context):
    print("Received event: " + json.dumps(event, indent=2))

    # Get the object from the event and show its content type
    bucket = event['Records'][0]['s3']['bucket']['name'] #our bucket
    key = urllib.parse.unquote_plus(event['Records'][0]['s3']['object']['key'], encoding='utf-8') #our file from test event
    print(bucket)
    print(key)

    img = {
            'S3Object': {
                'Bucket': bucket,
                'Name' : key
            }    
        }
        
    output: dict = rekognition.detect_text(Image=img)
    
    detected_text = ""
    for row in output['TextDetections']:
        detected_text = detected_text + row['DetectedText']
    
    with open("/tmp/output.txt", "w+") as f:
        f.write(detected_text)
        
    s3.upload_file("/tmp/output.txt", 'rekognitionoutput', 'output.txt')
    return output