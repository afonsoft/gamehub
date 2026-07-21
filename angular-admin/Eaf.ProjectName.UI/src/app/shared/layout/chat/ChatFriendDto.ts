import { ChatMessageDto, FriendshipDto } from '@shared/service-proxies/service-proxies';

export class ChatFriendDto extends FriendshipDto {
  messages: ChatMessageDto[];
  allPreviousMessagesLoaded = false;
  messagesLoaded = false;
}
