<div align="center">

# Lagrange.Milky

_[Milky](https://github.com/SaltifyDev/milky) protocol implementation based on [Lagrange.Core V2](https://github.com/LagrangeDev/LagrangeV2)_

</div>

## Implementation Progress

### communication

- [x] WebSocket
- [ ] WebHook

### api

#### system

- [ ] /get_login_info
- [x] /get_friend_list
- [x] /get_friend_info
- [x] /get_group_list
- [x] /get_group_info
- [x] /get_group_member_list
- [x] /get_group_member_info

#### message

- [x] /send_private_message
- [x] /send_group_message
- [ ] /get_message
- [ ] /get_history_messages
- [ ] /get_resource_temp_url
- [ ] /get_forwarded_messages
- [ ] /recall_private_message
- [ ] /recall_group_message

#### friend

- [ ] /send_friend_nudge
- [ ] /send_profile_like

#### group

- [ ] /set_group_name
- [ ] /set_group_avatar
- [ ] /set_group_member_card
- [ ] /set_group_member_special_title
- [ ] /set_group_member_admin
- [ ] /set_group_member_mute
- [ ] /set_group_whole_mute
- [ ] /kick_group_member
- [ ] /get_group_announcement_list
- [ ] /send_group_announcement
- [ ] /delete_group_announcement
- [ ] /quit_group
- [ ] /send_group_message_reaction
- [ ] /send_group_nudge

#### request

- [ ] /accept_request
- [ ] /reject_request

#### file

- [ ] /upload_private_file
- [ ] /upload_group_file
- [ ] /get_private_file_download_url
- [ ] /get_group_file_download_url
- [ ] /get_group_files
- [ ] /move_group_file
- [ ] /rename_group_file
- [ ] /delete_group_file
- [ ] /create_group_folder
- [ ] /rename_group_folder
- [ ] /delete_group_folder

### event

- [x] message_receive
- [ ] message_recall
- [ ] friend_request
- [ ] group_join_request
- [ ] group_invited_join_request
- [ ] group_invitation_request
- [ ] friend_nudge
- [ ] friend_file_upload
- [ ] group_admin_change
- [ ] group_essence_message_change
- [ ] group_member_increase
- [ ] group_member_decrease
- [ ] group_name_change
- [ ] group_message_reaction
- [ ] group_mute
- [ ] group_whole_mute
- [ ] group_nudge
- [ ] group_file_upload

### segment

#### incoming

- [x] text
- [x] mention
- [x] mention_all
- [ ] face
- [ ] reply
- [x] image
- [x] record
- [x] video
- [ ] forward
- [ ] market_face
- [ ] light_app
- [ ] xml

#### outgoing

- [x] text
- [ ] mention
- [ ] mention_all
- [ ] face
- [ ] reply
- [x] image
- [x] record
- [ ] video
- [ ] forward
