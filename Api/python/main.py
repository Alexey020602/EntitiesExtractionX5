import model

def main():
    minimal_example.model = None
    minimal_example.initialize(r"..\wwwroot\custom_ru_core_news_lg_with_9_labels_50_epochs")
    print(minimal_example.simple_function("Молоко"))


if __name__== "__main__":
    main()