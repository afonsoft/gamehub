import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-sdk-guide',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './sdk-guide.component.html',
})
export class SdkGuideComponent {
  readonly examples = {
    init: `<script src="https://gamehub.afonsoft.dev/gamehub-sdk.js"></script>
<script>
  await GameHubSDK.init();

  // Optional: listen to host events
  GameHubSDK.onPause(() => {
    game.pause();
  });

  GameHubSDK.onResume(() => {
    game.resume();
  });
</script>`,
    events: `// Call these as the game reaches each lifecycle moment
GameHubSDK.gameLoadingStarted();
GameHubSDK.gameLoadingFinished();
GameHubSDK.gameplayStart();

// When the player pauses or the game ends
GameHubSDK.gameplayStop();`,
    submit: `// Submit the player's best score after gameplay stops
try {
  await GameHubSDK.submitScore(1234);
} catch (err) {
  console.warn('Score not submitted:', err);
}`,
    ads: `// Commercial break: pause gameplay while the ad is shown
try {
  await GameHubSDK.commercialBreakRequested();
  // resume game
} catch (err) {
  // ad failed or was skipped, still resume
}

// Rewarded break: ask the player to watch for a bonus
const rewarded = await GameHubSDK.rewardedBreakRequested();
if (rewarded) {
  grantExtraLives(3);
}`,
    capabilities: `const caps = await GameHubSDK.getCapabilities();

if (caps.chat) {
  await GameHubSDK.chatConnect({
    gameId: 'my-game-id',
    matchId: 'match-123'
  });

  GameHubSDK.onChatMessage(msg => {
    showInGameChat(msg);
  });
}`,
    error: `async function safeBreak() {
  try {
    await GameHubSDK.commercialBreakRequested();
  } catch (err) {
    // Degrade gracefully: keep the game running
  }
}`,
  };
}
