db.getCollection("Proxies").find({})
db.getCollection("Proxies").updateOne(
{"_id":ObjectId("5cc6f6398f7d7f56b4833bad")},
{"$set":{"Account_Id":ObjectId("5cc6f85b3a8df535bd31b885")}})