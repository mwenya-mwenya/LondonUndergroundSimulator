import os
import sys

BASE_DIR = os.path.dirname(os.path.dirname(__file__)) 
GENERATED_DIR = os.path.join(BASE_DIR, "generated")
if GENERATED_DIR not in sys.path:
    sys.path.append(GENERATED_DIR)

import grpc
from concurrent import futures

import inference_pb2 as inference__pb2_import
import inference_pb2_grpc as inference__pb2_grpc_import

import joblib
import numpy as np
import lightgbm as lgb

frequency_model_file = sys.argv[1]

print(f"Loading model from: {frequency_model_file}")
model = lgb.Booster(model_file=frequency_model_file)


class FrequencyPredictorServicer(inference__pb2_grpc_import.FrequencyPredictorServicer):
    def Predict(self, request, context):

        features = np.array([[
            request.avg_waiting,
            request.max_waiting,
            request.avg_load_factor,
            request.current_train_count,

            request.day_of_week,
            request.hour,
            request.minute,
            request.time_minutes,

            request.hour_sin,
            request.hour_cos,
            request.minute_sin,
            request.minute_cos,
            request.day_sin,
            request.day_cos,

            float(request.is_morning_peak),
            float(request.is_lunch_peak),
            float(request.is_evening_peak),
            float(request.is_weekend),

            request.delta_waiting,
            request.delta_load_factor,

            request.expected_demand_multiplier
        ]])

        predictions = model.predict(features)
        prediction = predictions[0]

        print(f"Predicted frequency: {prediction}")

        return inference__pb2_import.SnapshotResponse(
            recommended_frequency=float(prediction)
        )
    def Ping(self, request, context):
        return inference__pb2_import.PingResponse(isHealthy=True)

    def Shutdown(self, request, context):
        # Stop the server in a background thread so the RPC can return
        import threading
        threading.Thread(target=self._stop_server).start()
        return inference__pb2_import.PingResponse(isHealthy=False)

    def _stop_server(self):
        import time
        time.sleep(0.5)  # Allow time for response frame to flush over socket
        self._server.stop(0)  


def serve():
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))

    servicer = FrequencyPredictorServicer()
    servicer._server = server   

    inference__pb2_grpc_import.add_FrequencyPredictorServicer_to_server(
        servicer, server
    )

    server.add_insecure_port("[::]:50051")
    print("PythonInferenceService running on port 50051...")
    server.start()
    server.wait_for_termination()

if __name__ == "__main__":
    serve()
