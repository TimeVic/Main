binable/kafka/bin/windows/kafka-topics.sh --create --topic quickstart-events --bootstrap-server localhost:9092

kafka-topics.sh --delete --topic=test-logs --bootstrap-server localhost:9092
kafka-topics.sh --delete --topic=prod-logs --bootstrap-server localhost:9092

kafka-topics.sh --create --topic=test-logs --if-not-exists --partitions=5 --bootstrap-server localhost:9092
kafka-topics.sh --create --topic=prod-logs --if-not-exists --partitions=5 --bootstrap-server localhost:9092
kafka-topics.sh --create --topic=prod-notifications --if-not-exists --partitions=5 --bootstrap-server localhost:9092

kafka-topics.sh --describe --topic timevic-production-logs --bootstrap-server localhost:9092
kafka-console-consumer.sh --topic timevic-production-logs --from-beginning --bootstrap-server localhost:9092
