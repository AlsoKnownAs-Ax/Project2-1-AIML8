from mlagents_envs.logging_util import get_logger
from typing import Deque, Dict
from collections import deque
from mlagents.trainers.ghost.trainer import GhostTrainer

logger = get_logger(__name__)


class GhostController:
    """
    GhostController contains a queue of team ids. GhostTrainers subscribe to the GhostController and query
    it to get the current learning team.  The GhostController cycles through team ids every 'swap_interval'
    which corresponds to the number of trainer steps between changing learning teams.
    The GhostController is a unique object and there can only be one per training run.
    """

    def __init__(self, maxlen: int = 10):
        """
        Create a GhostController.
        :param maxlen: Maximum number of GhostTrainers allowed in this GhostController
        """

        # Tracks last swap step for  each learning team because trainer
        # steps of all GhostTrainers do not increment together
        self._queue: Deque[int] = deque(maxlen=maxlen)
        self._learning_team: int = -1
        # Dict from team id to GhostTrainer for ELO calculation
        self._ghost_trainers: Dict[int, GhostTrainer] = {}
        # Signals to the trainer control to perform a hard change_training_team
        self._changed_training_team = False

    @property
    def get_learning_team(self) -> int:
        """
        Returns the current learning team.
        :return: The learning team id
        """
        return self._learning_team

    def should_reset(self) -> bool:
        """
        Whether or not team change occurred. Causes full reset in trainer_controller
        :return: The truth value of the team changing
        """
        changed_team = self._changed_training_team
        if self._changed_training_team:
            self._changed_training_team = False
        return changed_team

    def subscribe_team_id(self, team_id: int, trainer: GhostTrainer) -> None:
        """
        Given a team_id and trainer, add to queue and trainers if not already.
        The GhostTrainer is used later by the controller to get ELO ratings of agents.
        :param team_id: The team_id of an agent managed by this GhostTrainer
        :param trainer: A GhostTrainer that manages this team_id.
        """
        if team_id not in self._ghost_trainers:
            self._ghost_trainers[team_id] = trainer
            if self._learning_team < 0:
                self._learning_team = team_id
            else:
                self._queue.append(team_id)

    def change_training_team(self, step: int) -> None:
        """
        The current learning team is added to the end of the queue and then updated with the
        next in line.
        :param step: The step of the trainer for debugging
        """
        self._queue.append(self._learning_team)
        self._learning_team = self._queue.popleft()
        logger.debug(f"Learning team {self._learning_team} swapped on step {step}")
        self._changed_training_team = True


    def compute_elo_rating_changes(self, rating: float, result: float) -> float:
        """
        Calculates ELO using initial rating as base case. Does not modify opponent ratings.
        :param rating: Rating of the learning team.
        :param result: Win, loss, or draw from the perspective of the learning team.
        :return: The change in ELO.
        """
    # Get initial rating from any trainer since they all start with same value
    base_rating = next(iter(self._ghost_trainers.values())).initial_elo

    r1 = pow(10, rating / 400)
    r2 = pow(10, base_rating / 400)

    summed = r1 + r2
    e1 = r1 / summed

    return result - e1
