import re
version = None
with open("../../CHANGELOG.md") as f:
    for line in f:
        pattern = re.compile("## \[[0-9]*\.[0-9]*\.[0-9]*\] - .*$")
        if(pattern.match(line)):
           version = line.split("[")[1].split("]")[0]
           break;
if (version is None):
    raise Exception("Changelog is malformed")

import requests
from requests.auth import HTTPBasicAuth
import json

url = "https://hicservices.atlassian.net/rest/api/2/search"

auth = HTTPBasicAuth("jfriel001@dundee.ac.uk", "$JIRA_API_TOKEN")

headers = {
          "Accept": "application/json"
          }

query = {
          'jql': 'project = CM AND summary ~ "Release RDMP v'+ version + '"'
          }

response = requests.request(
           "GET",
              url,
                 headers=headers,
                    params=query,
                       auth=auth
                       )
if(response.status_code != 200):
    raise Exception("Unable to fetch Change Management Ticket")
STATUS = None
try:
    details = json.loads(response.text)
    STATUS = json.dumps(details['issues'][0]['fields']['status']['name']) 
except:
    raise Exception("Unable to validate CM ticket")

if(STATUS != '"Implement"'):
#if(STATUS != '"Post Implementation Review"'):
    raise Exception("CM ticket is in incorrect state " + STATUS)

