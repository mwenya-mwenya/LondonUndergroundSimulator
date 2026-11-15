import joblib

class FrequencyPredictor:
    def __init__(self):
        self.model = joblib.load("models/train_frequency.pkl")

    def predict(self, passenger_load, train_density, average_wait_time):
        features = [[passenger_load, train_density, average_wait_time]]
        prediction = self.model.predict(features)[0]
        return float(prediction)

