import model
import os

def main():
    model.fine_tuned_model = None
    path = r"/Users/Alexey/RiderProjects/EntitiesExtractionX5/Api/model"
    if os.path.isdir(path):
        model.initialize(path)
        print(model.simple_function("Молоко"))
    else:
        raise ValueError("Directory {path} not exists")


if __name__== "__main__":
    main()