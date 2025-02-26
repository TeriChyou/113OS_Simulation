import pygame
from consts import *
from core import *
# Initialize Pygame
pygame.init()

# Create Window
screen = pygame.display.set_mode((WIDTH, HEIGHT))
pygame.display.set_icon(pygame.image.load("resources/osIcon.png"))
pygame.display.set_caption("Teri's Simulated Operating System")

font = pygame.font.Font(None, 30)

running = True
while running:
    screen.fill((0, 0, 0))
    pygame.display.flip()

    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            running = False

pygame.quit()
