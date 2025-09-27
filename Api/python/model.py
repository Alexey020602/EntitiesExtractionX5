import spacy
const = None
model = None

def initialize(path: str) -> None:
    global const
    global model
    model = spacy.load(path)
    const = 4
    print("Initialize {const}")

def predict(query: str) -> list[tuple[int, int, str]]:
    doc = model(query)
    return [(ent.start_char, ent.end_char, ent.label_) for ent in doc.ents]
