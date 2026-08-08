-- Cassandra schema for web_ban_hoa keyspace and user_events table
-- Create keyspace
CREATE KEYSPACE IF NOT EXISTS web_ban_hoa
WITH replication = {'class': 'SimpleStrategy', 'replication_factor': '1'}
AND durable_writes = true;

-- Create user_events table
CREATE TABLE IF NOT EXISTS web_ban_hoa.user_events (
	user_id int,
	event_time timestamp,
	event_type text,
	product_id int,
	metadata text,
	PRIMARY KEY (user_id, event_time)
) WITH CLUSTERING ORDER BY (event_time DESC);
