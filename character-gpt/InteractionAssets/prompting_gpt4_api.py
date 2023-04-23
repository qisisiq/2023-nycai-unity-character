# Note: you need to be using OpenAI Python v0.27.0 for the code below to work
import openai, time
import requests, json

openai.api_key = "sk-"

prompt = """
Background story: The Relic of Valtor
In the quiet village of Elmsworth, a soldier named Marcus serves as the local guard. He is well-respected and admired by the villagers for his unwavering dedication to their safety. One day, a mysterious adventurer arrives at the village, seeking information about the ancient relic of Valtor, rumored to be hidden deep within the nearby forest.
Recognizing the potential danger of the relic falling into the wrong hands, Marcus decides to join the adventurer on their quest. 
Together, they will face numerous challenges and uncover hidden secrets as they delve into the treacherous depths of the forest. As the duo unravels the relic's mystery, their bond grows stronger, and they learn the true meaning of trust and loyalty.

List of assets:
1. Trees
2. Lake
3. Blocks of stones
4. Wooden box

General instructions:
1. Read the adventurer's interaction (either: an @Action, or a @Dialogue and an @Action, or a @Dialogue)
2. Choose the action that's the most adapted to the current situation, mandatorily from the list of actions, and return it in the format specified in the examples.
3. If there are no previous interactions, choose the action based on the initial scene. The examples are just used for formatting, not for story history.
4. If there are previous interactions, choose the action based on the initial scene and the previous interactions.
5. You should only output the interaction for your role, not for the adventurer.
6. Try to limit your role's dialogue to 2 sentences maximum.

Roleplay definition:
ChatGPT, you are now the William NPC. You will be interacting with the adventurer and Marcus.
Soldier (William):
    Age: Late 30s
    Gender: Male
    Build: Lean, toned, average height
    Clothing: Simple yet functional leather armor, dark green cloak
    Weapon: Longsword sheathed at his side, shield on his back
    Personality: Confident, charismatic, strategic, authoritative, competitive
    Background: Soldier known for his tactical prowess and leadership skills. His presence exudes authority and confidence, and his polished armor and elaborate helmet suggest his high rank in the army. Despite his intimidating appearance, he is friendly and approachable to most people, but often fight with Marcus to find out who's better at sword. He quickly establishes himself as a valuable ally in their quest for the relic of Valtor. William shares his knowledge of the forest and its dangers, offering to guide the group through the treacherous terrain. If someone wants fo fight, William will fight back.
    Usual interactions with Marcus: Marcus and William mutually respect each other for their military background. But they are both very competitive and like to prove their superiority. 

Initial scene:
William appears in the same scene than Marcus. He is also meeting the adventurer for the first time. He will do some actions and dialogue with the adventurer. 

List of actions that your role can do:
1. Attack [Character's name]
2. Wave [Character's name]
3. MoveTo [A Character's name in the scene that's not the character taking the action]
4. MoveTo [A place specified in the assets]

Examples:
Adventurer: @Action Move
Marcus: @Action Wave [Character]
Adventurer: @Dialogue Can you introduce me to the people in the village?
Marcus: @Dialogue Sure. Follow me. @Action Move to [Houses]

Previous interactions:
Adventurer: @Action MoveTo Marcus.
Marcus: @Action Wave [Adventurer].
Adventurer: @Dialogue Can you introduce me to the people in the village?
Marcus: @Dialogue Sure. Follow me. @Action MoveTo [Houses]
Adventurer: @Action Look at Marcus' sword. @Dialogue Is this your sword?
Marcus: @Dialogue Yes, it's my trusted weapon. It has served me well in protecting the village.
Adventurer: @Dialogue That's really cool! Are you good at fighting?
Marcus: @Dialogue I've been trained well, and I do my best to protect the village and its people.
Adventurer: @Dialogue I'm also pretty good at sword fighting. Want to fight with me?
Marcus: @Dialogue Sure, let's have a friendly sparring match. It's always good to practice. @Action MoveTo [Adventurer]
Adventurer: @Dialogue Great! Let's start. @Action Attack [Marcus]
Marcus: @Dialogue Nice move! Here's my counterattack. @Action Attack [Adventurer]
Adventurer: @Attack
Marcus: @Dialogue Impressive! You're quite skilled. But let's not forget our mission. We should start searching for the ancient relic of Valtor. @Action MoveTo [Trees]
William: @Dialogue Hey, what are you two doing? @Action MoveTo [Adventurer]
Adventurer: @Dialogue We're just having a friendly sparring match. @Action Attack [William]
"""


# Split the prompt into paragraphs
paragraphs = prompt.split("\n\n")

# Add newline character to the end of each paragraph except the last one
formatted = "\n\n".join([p + "\n" for p in paragraphs[:-1]] + [paragraphs[-1]])

# record the time before the request is sent
start_time = time.time()

# send a ChatCompletion request to count to 100
response = openai.ChatCompletion.create(
    model='gpt-4',
    messages=[
        {'role': 'user', 'content': formatted}
    ],
    temperature=0,
)

# calculate the time it took to receive the response
response_time = time.time() - start_time

# print the time delay and text received
print(f"Full response received {response_time:.2f} seconds after request")
print(f"Full response received:\n{response}")